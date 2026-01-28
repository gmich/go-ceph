using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class CheckBucketIndexRequest
{
    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    [JsonPropertyName("check-objects")]
    public bool? CheckObjects { get; set; }

    [JsonPropertyName("fix")]
    public bool? Fix { get; set; }
}

public sealed class CheckBucketIndexResponse
{
    [JsonPropertyName("invalid_multipart_entries")]
    public string[]? InvalidMultipartEntries { get; set; }

    [JsonPropertyName("check_result")]
    public CheckBucketIndexResult? CheckResult { get; set; }
}

public sealed class CheckBucketIndexResult
{
    [JsonPropertyName("existing_header")]
    public CheckBucketIndexHeader? ExistingHeader { get; set; }

    [JsonPropertyName("calculated_header")]
    public CheckBucketIndexHeader? CalculatedHeader { get; set; }
}

public sealed class CheckBucketIndexHeader
{
    [JsonPropertyName("usage")]
    public CheckBucketIndexUsage? Usage { get; set; }
}

public sealed class CheckBucketIndexUsage
{
    [JsonPropertyName("rgw.main")]
    public RgwUsage? RgwMain { get; set; }

    [JsonPropertyName("rgw.multimeta")]
    public RgwUsage? RgwMultimeta { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Check the index of an existing bucket.
    /// NOTE: to check multipart object accounting with check-objects, fix must be set to true.
    /// </summary>
    public Task<CheckBucketIndexResponse> CheckBucketIndexAsync(CheckBucketIndexRequest input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(input.Bucket))
        {
            throw new ArgumentException("Bucket name is required.", nameof(input));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "bucket", input.Bucket);
        AddQueryParameter(parameters, "check-objects", input.CheckObjects);
        AddQueryParameter(parameters, "fix", input.Fix);
        return SendAsync<CheckBucketIndexResponse>(HttpMethod.Get, "/bucket?index", parameters, cancellationToken);
    }
}
