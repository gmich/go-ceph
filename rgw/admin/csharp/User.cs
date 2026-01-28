using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public static class SubuserAccess
{
    public const string None = "";
    public const string Read = "read";
    public const string Write = "write";
    public const string ReadWrite = "readwrite";
    public const string Full = "full";

    public const string ReplyNone = "<none>";
    public const string ReplyRead = "read";
    public const string ReplyWrite = "write";
    public const string ReplyReadWrite = "read-write";
    public const string ReplyFull = "full-control";
}

public sealed class User
{
    [JsonPropertyName("user_id")]
    public string? Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("suspended")]
    public int? Suspended { get; set; }

    [JsonPropertyName("max_buckets")]
    public int? MaxBuckets { get; set; }

    [JsonPropertyName("subusers")]
    public List<SubuserSpec>? Subusers { get; set; }

    [JsonPropertyName("keys")]
    public List<UserKeySpec>? Keys { get; set; }

    [JsonPropertyName("swift_keys")]
    public List<SwiftKeySpec>? SwiftKeys { get; set; }

    [JsonPropertyName("caps")]
    public List<UserCapSpec>? Caps { get; set; }

    [JsonPropertyName("op_mask")]
    public string? OpMask { get; set; }

    [JsonPropertyName("default_placement")]
    public string? DefaultPlacement { get; set; }

    [JsonPropertyName("default_storage_class")]
    public string? DefaultStorageClass { get; set; }

    [JsonPropertyName("placement_tags")]
    public List<object>? PlacementTags { get; set; }

    [JsonPropertyName("bucket_quota")]
    public QuotaSpec? BucketQuota { get; set; }

    [JsonPropertyName("user_quota")]
    public QuotaSpec? UserQuota { get; set; }

    [JsonPropertyName("temp_url_keys")]
    public List<object>? TempUrlKeys { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("mfa_ids")]
    public List<object>? MfaIds { get; set; }

    [JsonIgnore]
    public string? KeyType { get; set; }

    [JsonIgnore]
    public string? Tenant { get; set; }

    [JsonIgnore]
    public bool? GenerateKey { get; set; }

    [JsonIgnore]
    public int? PurgeData { get; set; }

    [JsonIgnore]
    public bool? GenerateStat { get; set; }

    [JsonIgnore]
    public string? UserCaps { get; set; }

    [JsonPropertyName("stats")]
    public UserStat? Stat { get; set; }

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("account_root")]
    public bool? AccountRoot { get; set; }
}

public sealed class SubuserSpec
{
    [JsonPropertyName("id")]
    public string? Name { get; set; }

    [JsonPropertyName("permissions")]
    public string? Access { get; set; }

    [JsonIgnore]
    public bool? GenerateKey { get; set; }

    [JsonIgnore]
    public bool? GenerateAccessKey { get; set; }

    [JsonIgnore]
    public string? AccessKey { get; set; }

    [JsonIgnore]
    public string? SecretKey { get; set; }

    [JsonIgnore]
    public string? Secret { get; set; }

    [JsonIgnore]
    public bool? PurgeKeys { get; set; }

    [JsonIgnore]
    public string? KeyType { get; set; }

    [JsonIgnore]
    public bool? GenerateSecret { get; set; }
}

public sealed class SwiftKeySpec
{
    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("secret_key")]
    public string? SecretKey { get; set; }
}

public sealed class UserCapSpec
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("perm")]
    public string? Perm { get; set; }
}

public sealed class UserKeySpec
{
    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("access_key")]
    public string? AccessKey { get; set; }

    [JsonPropertyName("secret_key")]
    public string? SecretKey { get; set; }

    [JsonIgnore]
    public string? Uid { get; set; }

    [JsonIgnore]
    public string? SubUser { get; set; }

    [JsonIgnore]
    public string? KeyType { get; set; }

    [JsonIgnore]
    public bool? GenerateKey { get; set; }
}

public sealed class UserStat
{
    [JsonPropertyName("size")]
    public ulong? Size { get; set; }

    [JsonPropertyName("size_rounded")]
    public ulong? SizeRounded { get; set; }

    [JsonPropertyName("num_objects")]
    public ulong? NumObjects { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Retrieve a given object store user.
    /// </summary>
    public Task<User> GetUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id) && (user.Keys == null || user.Keys.Count == 0))
        {
            throw new ArgumentException("User id or access key is required.", nameof(user));
        }

        if (user.Keys is not null && user.Keys.Any(k => string.IsNullOrEmpty(k.AccessKey)))
        {
            throw new ArgumentException("Access key is required when providing user keys.", nameof(user));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        if (user.Keys is not null)
        {
            foreach (var key in user.Keys)
            {
                AddQueryParameter(parameters, "access-key", key.AccessKey);
            }
        }
        AddQueryParameter(parameters, "stats", user.GenerateStat);
        return SendAsync<User>(HttpMethod.Get, "/user", parameters, cancellationToken);
    }

    /// <summary>
    /// List all object store users.
    /// </summary>
    public Task<List<string>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        return SendAsync<List<string>>(HttpMethod.Get, "/metadata/user", parameters, cancellationToken);
    }

    /// <summary>
    /// Create a user in the object store.
    /// </summary>
    public Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }
        if (string.IsNullOrEmpty(user.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(user));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "display-name", user.DisplayName);
        AddQueryParameter(parameters, "default-placement", user.DefaultPlacement);
        AddQueryParameter(parameters, "email", user.Email);
        AddQueryParameter(parameters, "key-type", user.KeyType);
        if (user.Keys is not null)
        {
            foreach (var key in user.Keys)
            {
                AddQueryParameter(parameters, "access-key", key.AccessKey);
                AddQueryParameter(parameters, "secret-key", key.SecretKey);
            }
        }
        AddQueryParameter(parameters, "user-caps", user.UserCaps);
        AddQueryParameter(parameters, "tenant", user.Tenant);
        AddQueryParameter(parameters, "generate-key", user.GenerateKey);
        AddQueryParameter(parameters, "max-buckets", user.MaxBuckets);
        AddQueryParameter(parameters, "suspended", user.Suspended);
        AddQueryParameter(parameters, "op-mask", user.OpMask);
        AddQueryParameter(parameters, "account-id", user.AccountId);
        AddQueryParameter(parameters, "account-root", user.AccountRoot);
        return SendAsync<User>(HttpMethod.Put, "/user", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove a user from the object store.
    /// </summary>
    public Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "purge-data", user.PurgeData);
        return SendAsync(HttpMethod.Delete, "/user", parameters, cancellationToken);
    }

    /// <summary>
    /// Modify a user.
    /// http://docs.ceph.com/en/latest/radosgw/adminops/#modify-user
    /// </summary>
    public Task<User> ModifyUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(user.Id))
        {
            throw new ArgumentException("User id is required.", nameof(user));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", user.Id);
        AddQueryParameter(parameters, "display-name", user.DisplayName);
        AddQueryParameter(parameters, "default-placement", user.DefaultPlacement);
        AddQueryParameter(parameters, "email", user.Email);
        AddQueryParameter(parameters, "generate-key", user.GenerateKey);
        if (user.Keys is not null)
        {
            foreach (var key in user.Keys)
            {
                AddQueryParameter(parameters, "access-key", key.AccessKey);
                AddQueryParameter(parameters, "secret-key", key.SecretKey);
            }
        }
        AddQueryParameter(parameters, "key-type", user.KeyType);
        AddQueryParameter(parameters, "max-buckets", user.MaxBuckets);
        AddQueryParameter(parameters, "suspended", user.Suspended);
        AddQueryParameter(parameters, "op-mask", user.OpMask);
        AddQueryParameter(parameters, "account-id", user.AccountId);
        AddQueryParameter(parameters, "account-root", user.AccountRoot);
        return SendAsync<User>(HttpMethod.Post, "/user", parameters, cancellationToken);
    }
}
