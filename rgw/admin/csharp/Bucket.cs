using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class Bucket
{
    [JsonPropertyName("bucket")]
    public string? Name { get; set; }

    [JsonPropertyName("num_shards")]
    public ulong? NumShards { get; set; }

    [JsonPropertyName("tenant")]
    public string? Tenant { get; set; }

    [JsonPropertyName("zonegroup")]
    public string? Zonegroup { get; set; }

    [JsonPropertyName("placement_rule")]
    public string? PlacementRule { get; set; }

    [JsonPropertyName("explicit_placement")]
    public ExplicitPlacement? ExplicitPlacement { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("marker")]
    public string? Marker { get; set; }

    [JsonPropertyName("index_type")]
    public string? IndexType { get; set; }

    [JsonPropertyName("versioned")]
    public bool? Versioned { get; set; }

    [JsonPropertyName("versioning_enabled")]
    public bool? VersioningEnabled { get; set; }

    [JsonPropertyName("versioning")]
    public string? Versioning { get; set; }

    [JsonPropertyName("object_lock_enabled")]
    public bool? ObjectLockEnabled { get; set; }

    [JsonPropertyName("owner")]
    public string? Owner { get; set; }

    [JsonPropertyName("ver")]
    public string? Ver { get; set; }

    [JsonPropertyName("master_ver")]
    public string? MasterVer { get; set; }

    [JsonPropertyName("mtime")]
    public string? Mtime { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime? CreationTime { get; set; }

    [JsonPropertyName("max_marker")]
    public string? MaxMarker { get; set; }

    [JsonPropertyName("usage")]
    public BucketUsage? Usage { get; set; }

    [JsonPropertyName("bucket_quota")]
    public QuotaSpec? BucketQuota { get; set; }

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public bool? GenerateStat { get; set; }

    [JsonIgnore]
    public bool? Policy { get; set; }

    [JsonIgnore]
    public bool? PurgeObjects { get; set; }
}

public sealed class ExplicitPlacement
{
    [JsonPropertyName("data_pool")]
    public string? DataPool { get; set; }

    [JsonPropertyName("data_extra_pool")]
    public string? DataExtraPool { get; set; }

    [JsonPropertyName("index_pool")]
    public string? IndexPool { get; set; }
}

public sealed class BucketUsage
{
    [JsonPropertyName("rgw.main")]
    public RgwUsage? RgwMain { get; set; }

    [JsonPropertyName("rgw.multimeta")]
    public RgwUsage? RgwMultimeta { get; set; }
}

public sealed class RgwUsage
{
    [JsonPropertyName("size")]
    public ulong? Size { get; set; }

    [JsonPropertyName("size_actual")]
    public ulong? SizeActual { get; set; }

    [JsonPropertyName("size_utilized")]
    public ulong? SizeUtilized { get; set; }

    [JsonPropertyName("size_kb")]
    public ulong? SizeKb { get; set; }

    [JsonPropertyName("size_kb_actual")]
    public ulong? SizeKbActual { get; set; }

    [JsonPropertyName("size_kb_utilized")]
    public ulong? SizeKbUtilized { get; set; }

    [JsonPropertyName("num_objects")]
    public ulong? NumObjects { get; set; }
}

public sealed class Policy
{
    [JsonPropertyName("acl")]
    public PolicyAcl? Acl { get; set; }

    [JsonPropertyName("owner")]
    public PolicyOwner? Owner { get; set; }
}

public sealed class PolicyAcl
{
    [JsonPropertyName("acl_user_map")]
    public List<PolicyAclUserMapEntry>? AclUserMap { get; set; }

    [JsonPropertyName("acl_group_map")]
    public List<object>? AclGroupMap { get; set; }

    [JsonPropertyName("grant_map")]
    public List<PolicyGrantMapEntry>? GrantMap { get; set; }
}

public sealed class PolicyAclUserMapEntry
{
    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("acl")]
    public int? Acl { get; set; }
}

public sealed class PolicyGrantMapEntry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("grant")]
    public PolicyGrant? Grant { get; set; }
}

public sealed class PolicyGrant
{
    [JsonPropertyName("type")]
    public PolicyGrantType? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("permission")]
    public PolicyGrantPermission? Permission { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("group")]
    public int? Group { get; set; }

    [JsonPropertyName("url_spec")]
    public string? UrlSpec { get; set; }
}

public sealed class PolicyGrantType
{
    [JsonPropertyName("type")]
    public int? Type { get; set; }
}

public sealed class PolicyGrantPermission
{
    [JsonPropertyName("flags")]
    public int? Flags { get; set; }
}

public sealed class PolicyOwner
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Return the list of all buckets present in the object store.
    /// </summary>
    public Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        return SendAsync<List<string>>(HttpMethod.Get, "/bucket", parameters, cancellationToken);
    }

    /// <summary>
    /// Return various information about a specific bucket.
    /// </summary>
    public Task<Bucket> GetBucketInfoAsync(Bucket bucket, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucket.Name))
        {
            throw new ArgumentException("Bucket name is required.", nameof(bucket));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "bucket", bucket.Name);
        AddQueryParameter(parameters, "uid", bucket.UserId);
        AddQueryParameter(parameters, "stats", bucket.GenerateStat);
        return SendAsync<Bucket>(HttpMethod.Get, "/bucket", parameters, cancellationToken);
    }

    /// <summary>
    /// Get the bucket policy.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#get-bucket-or-object-policy
    /// </summary>
    public Task<Policy> GetBucketPolicyAsync(Bucket bucket, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucket.Name))
        {
            throw new ArgumentException("Bucket name is required.", nameof(bucket));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "bucket", bucket.Name);
        return SendAsync<Policy>(HttpMethod.Get, "/bucket?policy", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove a bucket from the object store.
    /// </summary>
    public Task RemoveBucketAsync(Bucket bucket, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bucket.Name))
        {
            throw new ArgumentException("Bucket name is required.", nameof(bucket));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "bucket", bucket.Name);
        AddQueryParameter(parameters, "purge-objects", bucket.PurgeObjects);
        return SendAsync(HttpMethod.Delete, "/bucket", parameters, cancellationToken);
    }
}
