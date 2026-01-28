using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    /// <summary>
    /// Generate or add keys to a user.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#create-key
    /// </summary>
    public Task<List<UserKeySpec>> CreateKeyAsync(UserKeySpec key, CancellationToken cancellationToken = default)
    {
        switch (key.KeyType)
        {
            case "swift":
                if (string.IsNullOrEmpty(key.SubUser))
                {
                    throw new ArgumentException("Subuser id is required for swift keys.", nameof(key));
                }
                break;
            case "s3":
            case "":
                if (string.IsNullOrEmpty(key.Uid))
                {
                    throw new ArgumentException("User id is required for s3 keys.", nameof(key));
                }
                break;
            default:
                throw new ArgumentException("Unsupported key type.", nameof(key));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", key.Uid);
        AddQueryParameter(parameters, "subuser", key.SubUser);
        AddQueryParameter(parameters, "access-key", key.AccessKey);
        AddQueryParameter(parameters, "secret-key", key.SecretKey);
        AddQueryParameter(parameters, "key-type", key.KeyType);
        AddQueryParameter(parameters, "generate-key", key.GenerateKey);
        return SendAsync<List<UserKeySpec>>(HttpMethod.Put, "/user?key", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove an existing key.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#remove-key
    /// </summary>
    public Task RemoveKeyAsync(UserKeySpec key, CancellationToken cancellationToken = default)
    {
        switch (key.KeyType)
        {
            case "swift":
                if (string.IsNullOrEmpty(key.SubUser))
                {
                    throw new ArgumentException("Subuser id is required for swift keys.", nameof(key));
                }
                break;
            case "s3":
            case "":
                if (string.IsNullOrEmpty(key.Uid))
                {
                    throw new ArgumentException("User id is required for s3 keys.", nameof(key));
                }
                if (string.IsNullOrEmpty(key.AccessKey))
                {
                    throw new ArgumentException("Access key is required for s3 keys.", nameof(key));
                }
                break;
            default:
                throw new ArgumentException("Unsupported key type.", nameof(key));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", key.Uid);
        AddQueryParameter(parameters, "subuser", key.SubUser);
        AddQueryParameter(parameters, "access-key", key.AccessKey);
        AddQueryParameter(parameters, "key-type", key.KeyType);
        return SendAsync(HttpMethod.Delete, "/user?key", parameters, cancellationToken);
    }
}
