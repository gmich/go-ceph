using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace GoCeph.Rgw.Admin;

public interface IRgwRequestSigner
{
    Task SignAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}

public sealed class RgwAdminException : Exception
{
    public RgwAdminException(HttpStatusCode statusCode, string responseBody)
        : base($"RGW admin request failed with status {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }
}

public partial class RgwAdminClient
{
    private const string AdminPath = "/admin";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Uri endpoint;
    private readonly HttpClient httpClient;
    private readonly IRgwRequestSigner? requestSigner;

    public RgwAdminClient(Uri endpoint, HttpClient httpClient, IRgwRequestSigner? requestSigner = null)
    {
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.requestSigner = requestSigner;
    }

    private static List<KeyValuePair<string, string>> CreateQueryParameters()
    {
        return new List<KeyValuePair<string, string>>
        {
            new("format", "json")
        };
    }

    private static void AddQueryParameter(List<KeyValuePair<string, string>> parameters, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            parameters.Add(new KeyValuePair<string, string>(name, value));
        }
    }

    private static void AddQueryParameter(List<KeyValuePair<string, string>> parameters, string name, bool? value)
    {
        if (value.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string>(name, value.Value.ToString().ToLowerInvariant()));
        }
    }

    private static void AddQueryParameter(List<KeyValuePair<string, string>> parameters, string name, int? value)
    {
        if (value.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string>(name, value.Value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static void AddQueryParameter(List<KeyValuePair<string, string>> parameters, string name, long? value)
    {
        if (value.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string>(name, value.Value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static void AddQueryParameter(List<KeyValuePair<string, string>> parameters, string name, ulong? value)
    {
        if (value.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string>(name, value.Value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var builder = new StringBuilder();
        var first = true;
        foreach (var parameter in parameters)
        {
            if (!first)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(parameter.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(parameter.Value));
            first = false;
        }

        return builder.ToString();
    }

    private Uri BuildAdminUri(string path, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var endpointRoot = endpoint.ToString().TrimEnd('/');
        var query = BuildQueryString(parameters);
        var separator = path.Contains("?", StringComparison.Ordinal) ? "&" : "?";
        var combined = $"{endpointRoot}{AdminPath}{path}{separator}{query}";
        return new Uri(combined, UriKind.Absolute);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, List<KeyValuePair<string, string>> parameters, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, BuildAdminUri(path, parameters));
        if (requestSigner is not null)
        {
            await requestSigner.SignAsync(request, cancellationToken).ConfigureAwait(false);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if ((int)response.StatusCode >= 300)
        {
            throw new RgwAdminException(response.StatusCode, payload);
        }

        return JsonSerializer.Deserialize<T>(payload, SerializerOptions) ?? throw new JsonException("Empty response payload.");
    }

    private async Task SendAsync(HttpMethod method, string path, List<KeyValuePair<string, string>> parameters, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, BuildAdminUri(path, parameters));
        if (requestSigner is not null)
        {
            await requestSigner.SignAsync(request, cancellationToken).ConfigureAwait(false);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if ((int)response.StatusCode >= 300)
        {
            throw new RgwAdminException(response.StatusCode, payload);
        }
    }
}
