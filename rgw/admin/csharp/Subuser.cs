using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    private static bool IsValidSubuserAccess(SubuserSpec subuser)
    {
        var access = subuser.Access ?? string.Empty;
        return access == SubuserAccess.None ||
               access == SubuserAccess.Read ||
               access == SubuserAccess.Write ||
               access == SubuserAccess.ReadWrite ||
               access == SubuserAccess.Full;
    }

    /// <summary>
    /// Create a subuser.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#create-subuser
    /// </summary>
    public Task CreateSubuserAsync(User user, SubuserSpec subuser, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }
        if (string.IsNullOrEmpty(subuser.Name))
        {
            throw new ArgumentException("Subuser name is required.", nameof(subuser));
        }
        if (!IsValidSubuserAccess(subuser))
        {
            throw new ArgumentException($"Invalid subuser access level '{subuser.Access}'.", nameof(subuser));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "subuser", subuser.Name);
        AddQueryParameter(parameters, "access", subuser.Access);
        AddQueryParameter(parameters, "access-key", subuser.AccessKey);
        AddQueryParameter(parameters, "secret-key", subuser.SecretKey);
        AddQueryParameter(parameters, "generate-secret", subuser.GenerateSecret);
        AddQueryParameter(parameters, "gen-access-key", subuser.GenerateAccessKey);
        AddQueryParameter(parameters, "key-type", subuser.KeyType);
        return SendAsync(HttpMethod.Put, "/user", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove a subuser.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#remove-subuser
    /// </summary>
    public Task RemoveSubuserAsync(User user, SubuserSpec subuser, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }
        if (string.IsNullOrEmpty(subuser.Name))
        {
            throw new ArgumentException("Subuser name is required.", nameof(subuser));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "subuser", subuser.Name);
        AddQueryParameter(parameters, "purge-keys", subuser.PurgeKeys);
        return SendAsync(HttpMethod.Delete, "/user", parameters, cancellationToken);
    }

    /// <summary>
    /// Modify a subuser.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#modify-subuser
    /// </summary>
    public Task ModifySubuserAsync(User user, SubuserSpec subuser, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }
        if (string.IsNullOrEmpty(subuser.Name))
        {
            throw new ArgumentException("Subuser name is required.", nameof(subuser));
        }
        if (!IsValidSubuserAccess(subuser))
        {
            throw new ArgumentException($"Invalid subuser access level '{subuser.Access}'.", nameof(subuser));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "subuser", subuser.Name);
        AddQueryParameter(parameters, "access", subuser.Access);
        AddQueryParameter(parameters, "secret", subuser.Secret);
        AddQueryParameter(parameters, "generate-secret", subuser.GenerateSecret);
        AddQueryParameter(parameters, "key-type", subuser.KeyType);
        return SendAsync(HttpMethod.Post, "/user", parameters, cancellationToken);
    }
}
