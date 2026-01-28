using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public partial class RgwAdminClient
{
    /// <summary>
    /// Get bucket quota for a user.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#get-bucket-quota
    /// </summary>
    public Task<QuotaSpec> GetBucketQuotaAsync(QuotaSpec quota, CancellationToken cancellationToken = default)
    {
        quota.QuotaType = "bucket";
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
    /// Set bucket quota for a user.
    /// https://docs.ceph.com/en/latest/radosgw/adminops/#set-bucket-quota
    /// </summary>
    public Task SetBucketQuotaAsync(QuotaSpec quota, CancellationToken cancellationToken = default)
    {
        quota.QuotaType = "bucket";
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
