using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public sealed class BucketListingSpec
{
    [JsonIgnore]
    public string? UserId { get; set; }

    [JsonIgnore]
    public bool? GenerateStat { get; set; }
}

public partial class RgwAdminClient
{
    /// <summary>
    /// Return the list of all buckets with stats (system admin API only).
    /// </summary>
    public Task<List<Bucket>> ListBucketsWithStatAsync(CancellationToken cancellationToken = default)
    {
        var listingSpec = new BucketListingSpec { GenerateStat = true };
        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "stats", listingSpec.GenerateStat);
        return SendAsync<List<Bucket>>(HttpMethod.Get, "/bucket", parameters, cancellationToken);
    }
}
