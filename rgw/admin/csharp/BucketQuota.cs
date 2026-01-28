using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    /// <summary>
    /// Set quota for an individual bucket.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#set-quota-for-an-individual-bucket
    /// </summary>
    public Task SetIndividualBucketQuotaAsync(QuotaSpec quota, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(quota.UserId))
        {
            throw new ArgumentException("User id is required.", nameof(quota));
        }

        if (string.IsNullOrEmpty(quota.Bucket))
        {
            throw new ArgumentException("Bucket name is required.", nameof(quota));
        }

        var parameters = CreateQueryParameters();
        AddQueryParameter(parameters, "bucket", quota.Bucket);
        AddQueryParameter(parameters, "uid", quota.UserId);
        AddQueryParameter(parameters, "enabled", quota.Enabled);
        AddQueryParameter(parameters, "max-size", quota.MaxSize);
        AddQueryParameter(parameters, "max-size-kb", quota.MaxSizeKb);
        AddQueryParameter(parameters, "max-objects", quota.MaxObjects);
        return SendAsync(HttpMethod.Put, "/bucket?quota", parameters, cancellationToken);
    }
}
