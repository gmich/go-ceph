using System.Security.Cryptography;
using System.Text;

namespace GoCeph.RgwAdmin;

internal sealed class AwsV4Signer
{
    private const string Algorithm = "AWS4-HMAC-SHA256";

    public void Sign(HttpRequestMessage request, string accessKey, string secretKey, DateTimeOffset timestamp)
    {
        if (request.RequestUri is null)
        {
            throw new InvalidOperationException("Request URI must be set before signing.");
        }

        const string region = "default";
        const string service = "s3";
        const string payloadHash = "UNSIGNED-PAYLOAD";

        var amzDate = timestamp.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'");
        var dateStamp = timestamp.UtcDateTime.ToString("yyyyMMdd");

        request.Headers.Remove("x-amz-date");
        request.Headers.Remove("x-amz-content-sha256");
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);

        var hostHeader = BuildHostHeader(request.RequestUri);
        request.Headers.Host = hostHeader;

        var canonicalRequest = BuildCanonicalRequest(request, hostHeader, amzDate, payloadHash);
        var hashedCanonicalRequest = HashHex(canonicalRequest);

        var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
        var stringToSign = $"{Algorithm}\n{amzDate}\n{credentialScope}\n{hashedCanonicalRequest}";

        var signingKey = GetSignatureKey(secretKey, dateStamp, region, service);
        var signature = HmacHex(signingKey, stringToSign);

        var signedHeaders = "host;x-amz-content-sha256;x-amz-date";
        var authorizationHeader = $"{Algorithm} Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
        request.Headers.Remove("Authorization");
        request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
    }

    private static string BuildCanonicalRequest(HttpRequestMessage request, string hostHeader, string amzDate, string payloadHash)
    {
        if (request.RequestUri is null)
        {
            throw new InvalidOperationException("Request URI must be set before signing.");
        }

        var canonicalUri = BuildCanonicalUri(request.RequestUri.AbsolutePath);
        var canonicalQueryString = BuildCanonicalQueryString(request.RequestUri.Query);
        var canonicalHeaders = new StringBuilder();
        canonicalHeaders.Append("host:").Append(hostHeader).Append('\n');
        canonicalHeaders.Append("x-amz-content-sha256:").Append(payloadHash).Append('\n');
        canonicalHeaders.Append("x-amz-date:").Append(amzDate).Append('\n');

        var signedHeaders = "host;x-amz-content-sha256;x-amz-date";

        return string.Join("\n", new[]
        {
            request.Method.Method,
            canonicalUri,
            canonicalQueryString,
            canonicalHeaders.ToString(),
            signedHeaders,
            payloadHash
        });
    }

    private static string BuildCanonicalUri(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
        {
            return "/";
        }

        var segments = absolutePath.Split('/', StringSplitOptions.None);
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = AwsEncode(segments[i], encodeSlash: false);
        }

        return string.Join("/", segments);
    }

    private static string BuildCanonicalQueryString(string query)
    {
        var trimmed = query.TrimStart('?');
        if (string.IsNullOrEmpty(trimmed))
        {
            return string.Empty;
        }

        var parameters = new List<(string Key, string Value)>();
        var parts = trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var pieces = part.Split('=', 2);
            var key = Uri.UnescapeDataString(pieces[0]);
            var value = pieces.Length > 1 ? Uri.UnescapeDataString(pieces[1]) : string.Empty;
            parameters.Add((key, value));
        }

        var ordered = parameters
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .ThenBy(item => item.Value, StringComparer.Ordinal)
            .Select(item => $"{AwsEncode(item.Key, encodeSlash: true)}={AwsEncode(item.Value, encodeSlash: true)}");

        return string.Join("&", ordered);
    }

    private static string AwsEncode(string value, bool encodeSlash)
    {
        var encoded = Uri.EscapeDataString(value ?? string.Empty);
        encoded = encoded.Replace("%7E", "~", StringComparison.Ordinal);
        if (!encodeSlash)
        {
            encoded = encoded.Replace("%2F", "/", StringComparison.Ordinal);
        }
        return encoded;
    }

    private static string BuildHostHeader(Uri requestUri)
    {
        if (requestUri.IsDefaultPort)
        {
            return requestUri.Host;
        }

        return $"{requestUri.Host}:{requestUri.Port}";
    }

    private static byte[] Hmac(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string HmacHex(byte[] key, string data)
    {
        var hash = Hmac(key, data);
        return ConvertToHex(hash);
    }

    private static byte[] GetSignatureKey(string secretKey, string dateStamp, string regionName, string serviceName)
    {
        var kDate = Hmac(Encoding.UTF8.GetBytes($"AWS4{secretKey}"), dateStamp);
        var kRegion = Hmac(kDate, regionName);
        var kService = Hmac(kRegion, serviceName);
        return Hmac(kService, "aws4_request");
    }

    private static string HashHex(string data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return ConvertToHex(hash);
    }

    private static string ConvertToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
