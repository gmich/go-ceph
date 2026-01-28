using System;
using System.Net.Http;
using Xunit;

namespace Rgw.Admin.Tests;

public class RgwAdminTests
{
    [Fact]
    public void BuildQueryPath_AppendsFormatAndHandlesBareQueryKeys()
    {
        var args = "format=json";

        var queryPath = QueryBuilder.BuildQueryPath("http://192.168.0.1", "/user", args);
        Assert.Equal("http://192.168.0.1/admin/user?format=json", queryPath);

        queryPath = QueryBuilder.BuildQueryPath("http://192.168.0.1", "/user?quota", args);
        Assert.Equal("http://192.168.0.1/admin/user?quota&format=json", queryPath);
    }

    [Fact]
    public void HandleStatusError_MapsKnownErrorCodes()
    {
        var fakeGetUserError =
            "{\"Code\":\"NoSuchUser\",\"RequestId\":\"tx000\",\"HostId\":\"host\"}";
        var fakeGetSubUserError =
            "{\"Code\":\"NoSuchSubUser\",\"RequestId\":\"tx001\",\"HostId\":\"host\"}";

        var err = StatusErrorParser.HandleStatusError(fakeGetUserError);
        var statusError = Assert.IsType<StatusError>(err);
        Assert.True(statusError.Is(ErrorReasons.NoSuchUser));

        err = StatusErrorParser.HandleStatusError(fakeGetSubUserError);
        statusError = Assert.IsType<StatusError>(err);
        Assert.True(statusError.Is(ErrorReasons.NoSuchSubUser));
    }

    [Fact]
    public void ClientConstructor_ValidatesEndpointAndCredentials()
    {
        Assert.Throws<MissingEndpointException>(() => new RgwAdminClient(string.Empty, "key", "secret"));
        Assert.Throws<MissingAccessKeyException>(() => new RgwAdminClient("http://192.168.0.1", "", "secret"));
        Assert.Throws<MissingSecretKeyException>(() => new RgwAdminClient("http://192.168.0.1", "key", ""));
    }

    [Fact]
    public void SignRequest_UsesUnsignedPayloadAndDeterministicSignature()
    {
        var client = new RgwAdminClient(
            "http://192.168.0.1",
            "AKIDEXAMPLE",
            "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY");
        var timestamp = new DateTimeOffset(2019, 12, 15, 12, 0, 0, TimeSpan.Zero);

        var request = client.BuildSignedRequest(HttpMethod.Get, "/user", "format=json", timestamp);

        Assert.True(request.Headers.TryGetValues("x-amz-content-sha256", out var payloadHashes));
        Assert.Contains("UNSIGNED-PAYLOAD", payloadHashes);

        Assert.True(request.Headers.TryGetValues("Authorization", out var authHeaders));
        var authHeader = Assert.Single(authHeaders);

        Assert.Equal(
            "AWS4-HMAC-SHA256 Credential=AKIDEXAMPLE/20191215/default/s3/aws4_request, " +
            "SignedHeaders=host;x-amz-content-sha256;x-amz-date, " +
            "Signature=e424a9dbaf86e28298f69e68fcfeb557f8cfc2a9a37069b281667a45bc3cfbc3",
            authHeader);
    }
}
