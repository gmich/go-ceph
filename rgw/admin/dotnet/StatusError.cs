using System;
using System.Text.Json;

namespace Rgw.Admin;

public sealed class StatusError : Exception
{
    public StatusError(string code, string requestId, string hostId)
        : base($"{code} {requestId} {hostId}".Trim())
    {
        Code = code;
        RequestId = requestId;
        HostId = hostId;
    }

    public string Code { get; }
    public string RequestId { get; }
    public string HostId { get; }

    public bool Is(string reason) => string.Equals(Code, reason, StringComparison.Ordinal);
}

internal sealed class StatusErrorPayload
{
    public string? Code { get; set; }
    public string? RequestId { get; set; }
    public string? HostId { get; set; }
}

public static class StatusErrorParser
{
    private const string UnmarshalError = "failed to unmarshal radosgw http response";

    public static Exception HandleStatusError(string responseBody)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<StatusErrorPayload>(responseBody);
            if (payload == null || string.IsNullOrWhiteSpace(payload.Code))
            {
                return new InvalidOperationException($"{UnmarshalError}. {responseBody}.");
            }

            return new StatusError(
                payload.Code,
                payload.RequestId ?? string.Empty,
                payload.HostId ?? string.Empty);
        }
        catch (JsonException ex)
        {
            return new InvalidOperationException($"{UnmarshalError}. {responseBody}.", ex);
        }
    }
}
