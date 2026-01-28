using System;
using System.Net.Http;

namespace Rgw.Admin;

public sealed class RgwAdminClient
{
    public const string AuthRegion = "default";
    public const string Service = "s3";

    public RgwAdminClient(string endpoint, string accessKey, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new MissingEndpointException();
        }

        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new MissingAccessKeyException();
        }

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new MissingSecretKeyException();
        }

        Endpoint = endpoint;
        AccessKey = accessKey;
        SecretKey = secretKey;
    }

    public string Endpoint { get; }
    public string AccessKey { get; }
    public string SecretKey { get; }

    public HttpRequestMessage BuildSignedRequest(HttpMethod method, string path, string args, DateTimeOffset timestamp)
    {
        var request = new HttpRequestMessage(method, QueryBuilder.BuildQueryPath(Endpoint, path, args));
        var signer = new AwsV4Signer();
        signer.Sign(request, AccessKey, SecretKey, AuthRegion, Service, timestamp);
        return request;
    }
}
