using System;

namespace Rgw.Admin;

public static class ErrorReasons
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

public sealed class MissingEndpointException : Exception
{
    public MissingEndpointException() : base("endpoint not set") { }
}

public sealed class MissingAccessKeyException : Exception
{
    public MissingAccessKeyException() : base("access key not set") { }
}

public sealed class MissingSecretKeyException : Exception
{
    public MissingSecretKeyException() : base("secret key not set") { }
}
