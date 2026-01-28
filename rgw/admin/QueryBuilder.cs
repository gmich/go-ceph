using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace GoCeph.Rgw.Admin
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class UrlParamAttribute : Attribute
    {
        public UrlParamAttribute(string name)
        {
            Name = name ?? string.Empty;
        }

        public string Name { get; }
    }

    public sealed class QueryParams
    {
        private readonly List<KeyValuePair<string, string>> _pairs = new();

        public void Add(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            _pairs.Add(new KeyValuePair<string, string>(name, value ?? string.Empty));
        }

        public string Encode()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < _pairs.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('&');
                }

                builder.Append(Uri.EscapeDataString(_pairs[i].Key));
                builder.Append('=');
                builder.Append(Uri.EscapeDataString(_pairs[i].Value));
            }

            return builder.ToString();
        }
    }

    public static class RgwAdminQueryBuilder
    {
        private const string QueryAdminPath = "/admin";

        public static string BuildQueryPath(string endpoint, string path, string args)
        {
            if (path.Contains("?"))
            {
                return $"{endpoint}{QueryAdminPath}{path}&{args}";
            }

            return $"{endpoint}{QueryAdminPath}{path}?{args}";
        }

        public static QueryParams ValueToUrlParams(object input, IReadOnlyCollection<string> acceptableFields)
        {
            var values = new QueryParams();
            values.Add("format", "json");
            AddObjectToParams(values, input, acceptableFields);
            return values;
        }

        public static void AddToUrlParams(QueryParams values, object input, IReadOnlyCollection<string> acceptableFields)
        {
            AddObjectToParams(values, input, acceptableFields);
        }

        private static void AddObjectToParams(QueryParams values, object input, IReadOnlyCollection<string> acceptableFields)
        {
            if (input == null)
            {
                return;
            }

            if (input is IEnumerable enumerable && input is not string)
            {
                foreach (var item in enumerable)
                {
                    AddObjectToParams(values, item, acceptableFields);
                }

                return;
            }

            var type = input.GetType();
            foreach (var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if (member is not PropertyInfo && member is not FieldInfo)
                {
                    continue;
                }

                var (name, ignore) = GetUrlParamName(member);
                if (ignore)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = member.Name;
                }

                var memberValue = member switch
                {
                    PropertyInfo property => property.GetIndexParameters().Length == 0 ? property.GetValue(input) : null,
                    FieldInfo field => field.GetValue(input),
                    _ => null
                };

                if (memberValue == null)
                {
                    continue;
                }

                var memberType = memberValue.GetType();
                if (IsSimpleType(memberType))
                {
                    AddIfAllowed(values, acceptableFields, name, memberValue);
                    continue;
                }

                var nullableUnderlying = Nullable.GetUnderlyingType(memberType);
                if (nullableUnderlying != null)
                {
                    AddIfAllowed(values, acceptableFields, name, memberValue);
                    continue;
                }

                if (memberValue is IEnumerable nestedEnumerable && memberValue is not string)
                {
                    foreach (var item in nestedEnumerable)
                    {
                        AddObjectToParams(values, item, acceptableFields);
                    }

                    continue;
                }

                AddObjectToParams(values, memberValue, acceptableFields);
            }
        }

        private static (string name, bool ignore) GetUrlParamName(MemberInfo member)
        {
            var attribute = member.GetCustomAttribute<UrlParamAttribute>();
            if (attribute == null)
            {
                return (member.Name, false);
            }

            if (attribute.Name == "-")
            {
                return (string.Empty, true);
            }

            return (attribute.Name, false);
        }

        private static void AddIfAllowed(QueryParams values, IReadOnlyCollection<string> acceptableFields, string name, object memberValue)
        {
            if (!Contains(acceptableFields, name))
            {
                return;
            }

            var valueText = FormatValue(memberValue);
            if (valueText.Length == 0)
            {
                return;
            }

            values.Add(name, valueText);
        }

        private static string FormatValue(object memberValue)
        {
            if (memberValue is bool flag)
            {
                return flag ? "true" : "false";
            }

            return Convert.ToString(memberValue, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static bool IsSimpleType(Type type)
        {
            return type == typeof(string) || type == typeof(bool) || type == typeof(int);
        }

        private static bool Contains(IReadOnlyCollection<string> acceptableFields, string name)
        {
            if (acceptableFields == null)
            {
                return false;
            }

            foreach (var field in acceptableFields)
            {
                if (string.Equals(field, name, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
