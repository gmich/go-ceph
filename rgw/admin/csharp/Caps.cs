using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    /// <summary>
    /// Add capabilities to a user and return the updated list of caps.
    /// </summary>
    public Task<List<UserCapSpec>> AddUserCapAsync(string userId, string userCap, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrEmpty(userCap))
        {
            throw new ArgumentException("User cap is required.", nameof(userCap));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", userId);
        AddQueryParameter(parameters, "user-caps", userCap);
        return SendAsync<List<UserCapSpec>>(HttpMethod.Put, "/user?caps", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove capabilities from a user and return the updated list of caps.
    /// </summary>
    public Task<List<UserCapSpec>> RemoveUserCapAsync(string userId, string userCap, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrEmpty(userCap))
        {
            throw new ArgumentException("User cap is required.", nameof(userCap));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", userId);
        AddQueryParameter(parameters, "user-caps", userCap);
        return SendAsync<List<UserCapSpec>>(HttpMethod.Delete, "/user?caps", parameters, cancellationToken);
    }
}
