using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class QuotaSpec
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    [JsonIgnore]
    public string? QuotaType { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("check_on_raw")]
    public bool? CheckOnRaw { get; set; }

    [JsonPropertyName("max_size")]
    public long? MaxSize { get; set; }

    [JsonPropertyName("max_size_kb")]
    public int? MaxSizeKb { get; set; }

    [JsonPropertyName("max_objects")]
    public long? MaxObjects { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Get the quota for a user.
    /// </summary>
    public Task<QuotaSpec> GetUserQuotaAsync(QuotaSpec quota, CancellationToken cancellationToken = default)
    {
        quota.QuotaType = "user";
        if (string.IsNullOrEmpty(quota.UserId))
        {
            throw new ArgumentException("User id is required.", nameof(quota));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", quota.UserId);
        AddQueryParameter(parameters, "quota-type", quota.QuotaType);
        return SendAsync<QuotaSpec>(HttpMethod.Get, "/user?quota", parameters, cancellationToken);
    }

    /// <summary>
    /// Set quota for a user.
    /// </summary>
    public Task SetUserQuotaAsync(QuotaSpec quota, CancellationToken cancellationToken = default)
    {
        quota.QuotaType = "user";
        if (string.IsNullOrEmpty(quota.UserId))
        {
            throw new ArgumentException("User id is required.", nameof(quota));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", quota.UserId);
        AddQueryParameter(parameters, "quota-type", quota.QuotaType);
        AddQueryParameter(parameters, "enabled", quota.Enabled);
        AddQueryParameter(parameters, "max-size", quota.MaxSize);
        AddQueryParameter(parameters, "max-size-kb", quota.MaxSizeKb);
        AddQueryParameter(parameters, "max-objects", quota.MaxObjects);
        return SendAsync(HttpMethod.Put, "/user?quota", parameters, cancellationToken);
    }
}
