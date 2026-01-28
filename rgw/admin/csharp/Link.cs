using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class BucketLinkInput
{
    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    [JsonPropertyName("bucket_id")]
    public string? BucketId { get; set; }

    [JsonPropertyName("uid")]
    public string? UserId { get; set; }

    [JsonPropertyName("new_bucket_name")]
    public string? NewBucketName { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Unlink a bucket from a specified user.
    /// </summary>
    public Task UnlinkBucketAsync(BucketLinkInput link, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(link.UserId))
        {
            throw new ArgumentException("User id is required.", nameof(link));
        }
        if (string.IsNullOrEmpty(link.Bucket))
        {
            throw new ArgumentException("Bucket name is required.", nameof(link));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", link.UserId);
        AddQueryParameter(parameters, "bucket", link.Bucket);
        return SendAsync(HttpMethod.Post, "/bucket", parameters, cancellationToken);
    }

    /// <summary>
    /// Link a bucket to a specified user, unlinking from any previous user.
    /// </summary>
    public Task LinkBucketAsync(BucketLinkInput link, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(link.UserId))
        {
            throw new ArgumentException("User id is required.", nameof(link));
        }
        if (string.IsNullOrEmpty(link.Bucket))
        {
            throw new ArgumentException("Bucket name is required.", nameof(link));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", link.UserId);
        AddQueryParameter(parameters, "bucket-id", link.BucketId);
        AddQueryParameter(parameters, "bucket", link.Bucket);
        AddQueryParameter(parameters, "new-bucket-name", link.NewBucketName);
        return SendAsync(HttpMethod.Put, "/bucket", parameters, cancellationToken);
    }
}
