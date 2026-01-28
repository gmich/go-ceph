using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class Usage
{
    [JsonPropertyName("entries")]
    public List<UsageEntry>? Entries { get; set; }

    [JsonPropertyName("summary")]
    public List<UsageSummary>? Summary { get; set; }

    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public string? Start { get; set; }

    [JsonIgnore]
    public string? End { get; set; }

    [JsonIgnore]
    public bool? ShowEntries { get; set; }

    [JsonIgnore]
    public bool? ShowSummary { get; set; }

    [JsonIgnore]
    public bool? RemoveAll { get; set; }
}

public sealed class UsageEntry
{
    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("buckets")]
    public List<UsageBucket>? Buckets { get; set; }
}

public sealed class UsageBucket
{
    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("epoch")]
    public ulong? Epoch { get; set; }

    [JsonPropertyName("owner")]
    public string? Owner { get; set; }

    [JsonPropertyName("categories")]
    public List<UsageCategory>? Categories { get; set; }
}

public sealed class UsageCategory
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("bytes_sent")]
    public ulong? BytesSent { get; set; }

    [JsonPropertyName("bytes_received")]
    public ulong? BytesReceived { get; set; }

    [JsonPropertyName("ops")]
    public ulong? Ops { get; set; }

    [JsonPropertyName("successful_ops")]
    public ulong? SuccessfulOps { get; set; }
}

public sealed class UsageSummary
{
    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("categories")]
    public List<UsageCategory>? Categories { get; set; }

    [JsonPropertyName("total")]
    public UsageTotal? Total { get; set; }
}

public sealed class UsageTotal
{
    [JsonPropertyName("bytes_sent")]
    public ulong? BytesSent { get; set; }

    [JsonPropertyName("bytes_received")]
    public ulong? BytesReceived { get; set; }

    [JsonPropertyName("ops")]
    public ulong? Ops { get; set; }

    [JsonPropertyName("successful_ops")]
    public ulong? SuccessfulOps { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Request bandwidth usage information on the object store.
    /// </summary>
    public Task<Usage> GetUsageAsync(Usage usage, CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", usage.UserId);
        AddQueryParameter(parameters, "start", usage.Start);
        AddQueryParameter(parameters, "end", usage.End);
        AddQueryParameter(parameters, "show-entries", usage.ShowEntries);
        AddQueryParameter(parameters, "show-summary", usage.ShowSummary);
        return SendAsync<Usage>(HttpMethod.Get, "/usage", parameters, cancellationToken);
    }

    /// <summary>
    /// Remove bandwidth usage information.
    /// </summary>
    public Task TrimUsageAsync(Usage usage, CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "uid", usage.UserId);
        AddQueryParameter(parameters, "start", usage.Start);
        AddQueryParameter(parameters, "end", usage.End);
        AddQueryParameter(parameters, "remove-all", usage.RemoveAll);
        return SendAsync(HttpMethod.Delete, "/usage", parameters, cancellationToken);
    }
}
