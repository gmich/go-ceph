using System.Text.Json;

namespace GoCeph.RgwAdmin;

public static class RgwAdminErrorReasons
{
    public const string UserAlreadyExists = "UserAlreadyExists";
    public const string NoSuchUser = "NoSuchUser";
    public const string InvalidAccessKey = "InvalidAccessKey";
    public const string InvalidSecretKey = "InvalidSecretKey";
    public const string InvalidKeyType = "InvalidKeyType";
    public const string KeyExists = "KeyExists";
    public const string EmailExists = "EmailExists";
    public const string InvalidCapability = "InvalidCapability";
    public const string SubuserExists = "SubuserExists";
    public const string NoSuchSubUser = "NoSuchSubUser";
    public const string InvalidAccess = "InvalidAccess";
    public const string IndexRepairFailed = "IndexRepairFailed";
    public const string BucketNotEmpty = "BucketNotEmpty";
    public const string ObjectRemovalFailed = "ObjectRemovalFailed";
    public const string BucketUnlinkFailed = "BucketUnlinkFailed";
    public const string BucketLinkFailed = "BucketLinkFailed";
    public const string NoSuchObject = "NoSuchObject";
    public const string IncompleteBody = "IncompleteBody";
    public const string NoSuchCap = "NoSuchCap";
    public const string InternalError = "InternalError";
    public const string AccessDenied = "AccessDenied";
    public const string NoSuchBucket = "NoSuchBucket";
    public const string NoSuchKey = "NoSuchKey";
    public const string InvalidArgument = "InvalidArgument";
    public const string Unknown = "Unknown";
    public const string SignatureDoesNotMatch = "SignatureDoesNotMatch";
    public const string AccountAlreadyExists = "AccountAlreadyExists";
}

public sealed class RgwAdminStatusException : Exception
{
    public string? Code { get; }
    public string? RequestId { get; }
    public string? HostId { get; }

    public RgwAdminStatusException(string? code, string? requestId, string? hostId)
        : base($"{code} {requestId} {hostId}".Trim())
    {
        Code = code;
        RequestId = requestId;
        HostId = hostId;
    }

    public bool IsReason(string reason) => string.Equals(Code, reason, StringComparison.Ordinal);
}

public sealed class RgwAdminResponseException : Exception
{
    public RgwAdminResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

internal sealed record StatusError(string? Code, string? RequestId, string? HostId);

internal static class ErrorHandling
{
    private const string UnmarshalError = "failed to unmarshal radosgw http response";

    public static Exception HandleStatusError(byte[] responseBody)
    {
        try
        {
            var statusError = JsonSerializer.Deserialize<StatusError>(responseBody);
            return new RgwAdminStatusException(statusError?.Code, statusError?.RequestId, statusError?.HostId);
        }
        catch (Exception ex)
        {
            var responseText = System.Text.Encoding.UTF8.GetString(responseBody);
            return new RgwAdminResponseException($"{UnmarshalError}. {responseText}", ex);
        }
    }
}
