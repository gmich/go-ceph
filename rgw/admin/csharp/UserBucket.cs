using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    /// <summary>
    /// Return the list of all buckets for a user without stats.
    /// </summary>
    public Task<List<string>> ListUsersBucketsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", userId);
        AddQueryParameter(parameters, "stats", false);
        return SendAsync<List<string>>(HttpMethod.Get, "/bucket", parameters, cancellationToken);
    }

    /// <summary>
    /// Return the list of all buckets for a user with stats.
    /// </summary>
    public Task<List<Bucket>> ListUsersBucketsWithStatAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", userId);
        AddQueryParameter(parameters, "stats", true);
        return SendAsync<List<Bucket>>(HttpMethod.Get, "/bucket", parameters, cancellationToken);
    }
}
