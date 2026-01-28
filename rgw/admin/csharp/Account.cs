using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

/// <summary>
/// Represents an RGW account.
/// </summary>
public sealed class Account
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("tenant")]
    public string? Tenant { get; set; }

    [JsonPropertyName("max_users")]
    public long? MaxUsers { get; set; }

    [JsonPropertyName("max_roles")]
    public long? MaxRoles { get; set; }

    [JsonPropertyName("max_groups")]
    public long? MaxGroups { get; set; }

    [JsonPropertyName("max_access_keys")]
    public long? MaxAccessKeys { get; set; }

    [JsonPropertyName("max_buckets")]
    public long? MaxBuckets { get; set; }

    [JsonPropertyName("quota")]
    public QuotaSpec? Quota { get; set; }

    [JsonPropertyName("bucket_quota")]
    public QuotaSpec? BucketQuota { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Create a new RGW account.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#create-account
    /// </summary>
    public Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "id", account.Id);
        AddQueryParameter(parameters, "name", account.Name);
        AddQueryParameter(parameters, "email", account.Email);
        AddQueryParameter(parameters, "tenant", account.Tenant);
        AddQueryParameter(parameters, "max-users", account.MaxUsers);
        AddQueryParameter(parameters, "max-roles", account.MaxRoles);
        AddQueryParameter(parameters, "max-groups", account.MaxGroups);
        AddQueryParameter(parameters, "max-access-keys", account.MaxAccessKeys);
        AddQueryParameter(parameters, "max-buckets", account.MaxBuckets);
        return SendAsync<Account>(HttpMethod.Post, "/account", parameters, cancellationToken);
    }

    /// <summary>
    /// Get RGW account details.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#get-account-info
    /// </summary>
    public Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            throw new ArgumentException("Account id is required.", nameof(accountId));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "id", accountId);
        return SendAsync<Account>(HttpMethod.Get, "/account", parameters, cancellationToken);
    }

    /// <summary>
    /// Delete an RGW account.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#remove-account
    /// </summary>
    public Task DeleteAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accountId))
        {
            throw new ArgumentException("Account id is required.", nameof(accountId));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "id", accountId);
        return SendAsync(HttpMethod.Delete, "/account", parameters, cancellationToken);
    }

    /// <summary>
    /// Modify an RGW account.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#modify-account
    /// </summary>
    public Task<Account> ModifyAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(account.Id))
        {
            throw new ArgumentException("Account id is required.", nameof(account));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "id", account.Id);
        AddQueryParameter(parameters, "name", account.Name);
        AddQueryParameter(parameters, "email", account.Email);
        AddQueryParameter(parameters, "tenant", account.Tenant);
        AddQueryParameter(parameters, "max-users", account.MaxUsers);
        AddQueryParameter(parameters, "max-roles", account.MaxRoles);
        AddQueryParameter(parameters, "max-groups", account.MaxGroups);
        AddQueryParameter(parameters, "max-access-keys", account.MaxAccessKeys);
        AddQueryParameter(parameters, "max-buckets", account.MaxBuckets);
        return SendAsync<Account>(HttpMethod.Put, "/account", parameters, cancellationToken);
    }
}
