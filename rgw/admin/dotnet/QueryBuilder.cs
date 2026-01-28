using System;

namespace Rgw.Admin;

public static class QueryBuilder
{
    private const string QueryAdminPath = "/admin";

    public static string BuildQueryPath(string endpoint, string path, string args)
    {
        if (path.Contains("?", StringComparison.Ordinal))
        {
            return $"{endpoint}{QueryAdminPath}{path}&{args}";
        }

        return $"{endpoint}{QueryAdminPath}{path}?{args}";
    }
}
