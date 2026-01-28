using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Rgw.Admin;

public sealed class AwsV4Signer
{
    private const string Algorithm = "AWS4-HMAC-SHA256";
    private const string UnsignedPayload = "UNSIGNED-PAYLOAD";

    public void Sign(HttpRequestMessage request, string accessKey, string secretKey, string region, string service, DateTimeOffset timestamp)
    {
        var amzDate = timestamp.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = timestamp.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        request.Headers.Remove("x-amz-date");
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.Remove("x-amz-content-sha256");
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", UnsignedPayload);

        var canonicalRequest = BuildCanonicalRequest(request, amzDate);
        var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
        var stringToSign = BuildStringToSign(amzDate, credentialScope, canonicalRequest);
        var signingKey = BuildSigningKey(secretKey, dateStamp, region, service);
        var signature = ToHex(HmacSha256(signingKey, stringToSign));

        var signedHeaders = GetSignedHeaders();
        var authorizationHeader =
            $"{Algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

        request.Headers.Remove("Authorization");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
    }

    private static string BuildCanonicalRequest(HttpRequestMessage request, string amzDate)
    {
        var canonicalUri = request.RequestUri?.AbsolutePath ?? "/";
        var canonicalQuery = BuildCanonicalQueryString(request.RequestUri?.Query ?? string.Empty);
        var canonicalHeaders = BuildCanonicalHeaders(request);
        var signedHeaders = GetSignedHeaders();

        return string.Join("\n", new[]
        {
            request.Method.Method,
            canonicalUri,
            canonicalQuery,
            canonicalHeaders,
            signedHeaders,
            UnsignedPayload,
        });
    }

    private static string BuildStringToSign(string amzDate, string credentialScope, string canonicalRequest)
    {
        var hashedRequest = HashSha256(canonicalRequest);
        return string.Join("\n", new[] { Algorithm, amzDate, credentialScope, hashedRequest });
    }

    private static string BuildCanonicalHeaders(HttpRequestMessage request)
    {
        var host = request.RequestUri?.IsDefaultPort == false
            ? $"{request.RequestUri.Host}:{request.RequestUri.Port}"
            : request.RequestUri?.Host ?? string.Empty;

        var headers = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["host"] = host,
            ["x-amz-content-sha256"] = UnsignedPayload,
            ["x-amz-date"] = request.Headers.GetValues("x-amz-date").Single(),
        };

        return string.Join("\n", headers.Select(kvp => $"{kvp.Key}:{kvp.Value}")) + "\n";
    }

    private static string GetSignedHeaders() => "host;x-amz-content-sha256;x-amz-date";

    private static string BuildCanonicalQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
        {
            return string.Empty;
        }

        var query = queryString.StartsWith("?", StringComparison.Ordinal)
            ? queryString.Substring(1)
            : queryString;

        var pairs = new List<KeyValuePair<string, string>>();
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex >= 0)
            {
                var key = Uri.UnescapeDataString(part.Substring(0, separatorIndex));
                var value = Uri.UnescapeDataString(part.Substring(separatorIndex + 1));
                pairs.Add(new KeyValuePair<string, string>(key, value));
            }
            else
            {
                var key = Uri.UnescapeDataString(part);
                pairs.Add(new KeyValuePair<string, string>(key, string.Empty));
            }
        }

        return string.Join("&",
            pairs
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .ThenBy(p => p.Value, StringComparer.Ordinal)
                .Select(p => $"{Encode(p.Key)}={Encode(p.Value)}"));
    }

    private static string Encode(string value)
    {
        return Uri.EscapeDataString(value).Replace("%7E", "~", StringComparison.Ordinal);
    }

    private static byte[] BuildSigningKey(string secretKey, string dateStamp, string region, string service)
    {
        var kDate = HmacSha256(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateStamp);
        var kRegion = HmacSha256(kDate, region);
        var kService = HmacSha256(kRegion, service);
        return HmacSha256(kService, "aws4_request");
    }

    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string HashSha256(string data)
    {
        using var sha256 = SHA256.Create();
        return ToHex(sha256.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    private static string ToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
