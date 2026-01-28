using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class StorageBackend
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cluster_id")]
    public string? ClusterId { get; set; }
}

public sealed class InfoSpec
{
    [JsonPropertyName("storage_backends")]
    public List<StorageBackend>? StorageBackends { get; set; }
}

public sealed class Info
{
    [JsonPropertyName("info")]
    public InfoSpec? InfoSpec { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Get RGW info.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#info
    /// </summary>
    public Task<Info> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateQueryParameters();
        return SendAsync<Info>(HttpMethod.Get, "/info", parameters, cancellationToken);
    }
}
