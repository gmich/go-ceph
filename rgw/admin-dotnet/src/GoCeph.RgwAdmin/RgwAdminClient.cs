using System.Net.Http;
using System.Text;

namespace GoCeph.RgwAdmin;

public sealed class RgwAdminClient
{
    private const string QueryAdminPath = "/admin";
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(3);
    private readonly AwsV4Signer _signer = new();

    public RgwAdminClient(string endpoint, string accessKey, string secretKey, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("endpoint not set", nameof(endpoint));
        }

        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new ArgumentException("access key not set", nameof(accessKey));
        }

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("secret key not set", nameof(secretKey));
        }

        Endpoint = endpoint.TrimEnd('/');
        AccessKey = accessKey;
        SecretKey = secretKey;
        HttpClient = httpClient ?? new HttpClient { Timeout = ConnectionTimeout };
    }

    public string Endpoint { get; }
    public string AccessKey { get; }
    public string SecretKey { get; }
    public HttpClient HttpClient { get; }

    public async Task<byte[]> CallAsync(
        HttpMethod httpMethod,
        string path,
        IReadOnlyDictionary<string, string?>? args = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildQueryPath(Endpoint, path, args);
        using var request = new HttpRequestMessage(httpMethod, requestUri);

        _signer.Sign(request, AccessKey, SecretKey, DateTimeOffset.UtcNow);

        using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

        if ((int)response.StatusCode >= 300)
        {
            throw ErrorHandling.HandleStatusError(responseBody);
        }

        return responseBody;
    }

    private static string BuildQueryPath(string endpoint, string path, IReadOnlyDictionary<string, string?>? args)
    {
        var encoded = EncodeQuery(args);
        if (path.Contains("?", StringComparison.Ordinal))
        {
            return $"{endpoint}{QueryAdminPath}{path}&{encoded}";
        }

        return $"{endpoint}{QueryAdminPath}{path}?{encoded}";
    }

    private static string EncodeQuery(IReadOnlyDictionary<string, string?>? args)
    {
        if (args is null || args.Count == 0)
        {
            return string.Empty;
        }

        var ordered = args
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{AwsEncode(pair.Key)}={AwsEncode(pair.Value ?? string.Empty)}");

        return string.Join("&", ordered);
    }

    private static string AwsEncode(string value)
    {
        var encoded = Uri.EscapeDataString(value ?? string.Empty);
        return encoded.Replace("%7E", "~", StringComparison.Ordinal);
    }
}
