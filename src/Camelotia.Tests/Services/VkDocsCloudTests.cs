// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using Camelotia.Services.Apis.Vk;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="VkDocsCloud"/> against stubbed VK API and VK ID replies.</summary>
public sealed class VkDocsCloudTests
{
    /// <summary>The VK user identifier the tests sign in as.</summary>
    private const string UserId = "42";

    /// <summary>The document name the tests list and upload.</summary>
    private const string FileName = "a.txt";

    /// <summary>The text the exception constructor tests use.</summary>
    private const string Message = "message";

    /// <summary>The number of requests an upload makes: server, upload, save.</summary>
    private const int UploadRequestCount = 3;

    /// <summary>The number of requests a sign-in then two sign-outs make: exchange, revoke.</summary>
    private const int SignInRequestCount = 2;

    /// <summary>The docs.save reply.</summary>
    private const string Saved = """{"response":{"type":"doc","doc":{"id":8,"owner_id":42,"title":"a.txt","size":3,"date":0,"url":"u"}}}""";

    /// <summary>The documents reply: one document.</summary>
    private const string Documents = """{"response":{"count":1,"items":[{"id":7,"owner_id":42,"title":"a.txt","size":3,"date":1577836800,"url":"https://files.example/7"}]}}""";

    /// <summary>The upload server reply.</summary>
    private const string UploadServer = """{"response":{"upload_url":"https://upload.example/doc"}}""";

    /// <summary>Lists the user's documents as files.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_MapsDocuments()
    {
        var handler = new StubHttpHandler().On(HttpMethod.Get, "/method/docs.get?v=5.199&owner_id=42", static _ => StubHttpHandler.Json(Documents));
        using var services = new CloudTestServices(handler);
        using var cloud = CreateSignedIn(services);

        List<FileModel> files = [.. await cloud.GetFiles("/", CancellationToken.None)];

        await Assert.That(files.Count).IsEqualTo(1);
        await Assert.That(files[0].Name).IsEqualTo(FileName);
        await Assert.That(files[0].Path).IsEqualTo("7");
        await Assert.That(files[0].Modified).IsEqualTo(TestValues.Moment);
        await Assert.That(handler.Requests[0].Headers.Authorization!.ToString()).IsEqualTo($"Bearer {TestValues.Token}");
    }

    /// <summary>Throws when VK reports a failure inside a successful reply.</summary>
    /// <param name="reply">The VK reply.</param>
    /// <param name="code">The expected error code.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("""{"error":{"error_code":5,"error_msg":"User authorization failed"}}""", 5)]
    [Arguments("{}", 0)]
    public async Task GetFiles_ThrowsTheVkError(string reply, int code)
    {
        var handler = new StubHttpHandler().On(HttpMethod.Get, "/method/docs.get", _ => StubHttpHandler.Json(reply));
        using var services = new CloudTestServices(handler);
        using var cloud = CreateSignedIn(services);

        var error = await Assert.That(async () => await cloud.GetFiles("/", CancellationToken.None)).Throws<VkApiException>();

        await Assert.That(error!.ErrorCode).IsEqualTo(code);
    }

    /// <summary>Downloads a listed document, and fails for one that is gone.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DownloadFile_CopiesTheDocument()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/method/docs.get", static _ => StubHttpHandler.Json(Documents))
            .On(HttpMethod.Get, "/7", static _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(TestValues.Content) });
        using var services = new CloudTestServices(handler);
        using var cloud = CreateSignedIn(services);
        await using var target = new MemoryStream();

        await cloud.DownloadFile("7", target, CancellationToken.None);

        await Assert.That(target.ToArray()).IsEquivalentTo(TestValues.Content);
        _ = await Assert.That(async () => await cloud.DownloadFile("8", target, CancellationToken.None)).Throws<FileNotFoundException>();
    }

    /// <summary>Uploads to the upload server, then saves the document.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task UploadFile_SavesTheUploadedFile()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/method/docs.getWallUploadServer", static _ => StubHttpHandler.Json(UploadServer))
            .On(HttpMethod.Post, "/doc", static _ => StubHttpHandler.Json("""{"file":"uploaded"}"""))
            .On(HttpMethod.Post, "/method/docs.save?v=5.199&file=uploaded&title=a.txt", static _ => StubHttpHandler.Json(Saved));
        using var services = new CloudTestServices(handler);
        using var cloud = CreateSignedIn(services);
        await using var content = new MemoryStream(TestValues.Content);

        await cloud.UploadFile("/", content, FileName, CancellationToken.None);

        await Assert.That(handler.Requests.Count).IsEqualTo(UploadRequestCount);
        await Assert.That(handler.Bodies[1]).Contains("name=file");
    }

    /// <summary>Throws the upload server's message when it refuses the file.</summary>
    /// <param name="reply">The upload server reply.</param>
    /// <param name="message">The expected message.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("""{"error":"too big"}""", "too big")]
    [Arguments("{}", "VK did not accept a.txt.")]
    public async Task UploadFile_ThrowsWhenTheServerRefuses(string reply, string message)
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/method/docs.getWallUploadServer", static _ => StubHttpHandler.Json(UploadServer))
            .On(HttpMethod.Post, "/doc", _ => StubHttpHandler.Json(reply));
        using var services = new CloudTestServices(handler);
        using var cloud = CreateSignedIn(services);
        await using var content = new MemoryStream(TestValues.Content);

        var error = await Assert.That(async () => await cloud.UploadFile("/", content, FileName, CancellationToken.None)).Throws<VkApiException>();

        await Assert.That(error!.Message).IsEqualTo(message);
    }

    /// <summary>Signs in through VK ID with PKCE, then signs out and revokes the token.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task OAuth_ExchangesTheCodeAndLogoutRevokesIt()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Post, "/oauth2/auth", static _ => StubHttpHandler.Json($$"""{"access_token":"{{TestValues.OtherToken}}","user_id":42}"""))
            .On(HttpMethod.Post, "/oauth2/logout", static _ => StubHttpHandler.Json("""{"response":1}"""));
        using var services = new CloudTestServices(handler);
        var authenticator = new FakeAuthenticator();
        using var cloud = new VkDocsCloud(new CloudParameters(), authenticator, services.Configuration.VkDocs, services.Apis.Vk, services.Apis.VkId);

        await cloud.OAuth(CancellationToken.None);
        var user = cloud.Parameters.User;
        await cloud.Logout(CancellationToken.None);
        await cloud.Logout(CancellationToken.None);

        await Assert.That(user).IsEqualTo(UserId);
        await Assert.That(cloud.Parameters.Token).IsNull();
        await Assert.That(handler.Bodies[0]).Contains("device_id=the-device");
        await Assert.That(handler.Bodies[1]).Contains($"access_token={TestValues.OtherToken}");
        await Assert.That(handler.Requests.Count).IsEqualTo(SignInRequestCount);
        await Assert.That(authenticator.SignInPages[0].Host).IsEqualTo("id.vk.ru");
    }

    /// <summary>Rejects a redirect whose state differs, or that lacks a code or device.</summary>
    /// <param name="query">The redirect query.</param>
    /// <param name="message">The expected failure message.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("code=c&state=other", "The sign-in reply did not match this request.")]
    [Arguments("state={state}&error_description=Denied", "Denied")]
    [Arguments("state={state}", "VK did not return a sign-in code.")]
    [Arguments("code=c&state={state}", "VK did not return a device identifier.")]
    public async Task OAuth_RejectsABadRedirect(string query, string message)
    {
        using var services = new CloudTestServices(new());
        var authenticator = new FakeAuthenticator { Query = query };
        using var cloud = new VkDocsCloud(new CloudParameters(), authenticator, services.Configuration.VkDocs, services.Apis.Vk, services.Apis.VkId);

        var error = await Assert.That(async () => await cloud.OAuth(CancellationToken.None)).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).IsEqualTo(message);
    }

    /// <summary>Reports its capabilities and refuses what VK no longer allows.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeVkDocuments()
    {
        using var services = new CloudTestServices(new());
        using var cloud = CreateSignedIn(services);

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsTrue();
        await Assert.That(cloud.Name).IsEqualTo("VkDocs");
        await Assert.That(cloud.Id).IsEqualTo(cloud.Parameters.Id);
        await Assert.That(cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(cloud.Size).IsNull();
        await Assert.That(cloud.InitialPath).IsEqualTo("/");
        await Assert.That(cloud.CanCreateFolder).IsFalse();
        await Assert.That(cloud.SupportsOAuth).IsTrue();
        await Assert.That(cloud.SupportsDirectAuth).IsFalse();
        await Assert.That(cloud.SupportsHostAuth).IsFalse();
        await Assert.That(await cloud.GetBreadCrumbs("/", CancellationToken.None)).IsEmpty();
        _ = await Assert.That(async () => await cloud.DirectAuth(UserId, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.HostAuth("host", 1, UserId, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.CreateFolder("/", "a", CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.RenameFile("7", "b", CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.Delete("7", isFolder: false, CancellationToken.None)).Throws<NotSupportedException>();
    }

    /// <summary>Starts signed out without a token or user, and refuses calls.</summary>
    /// <param name="token">The saved token.</param>
    /// <param name="user">The saved user.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(null, UserId)]
    [Arguments("saved", null)]
    public async Task SignedOut_RefusesCalls(string? token, string? user)
    {
        using var services = new CloudTestServices(new());
        using var cloud = new VkDocsCloud(new CloudParameters { Token = token, User = user }, new FakeAuthenticator(), services.Configuration.VkDocs, services.Apis.Vk, services.Apis.VkId);

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
        _ = await Assert.That(async () => await cloud.GetFiles("/", CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Refuses an upload without a saved token.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task UploadFile_RequiresAToken()
    {
        using var services = new CloudTestServices(new());
        using var cloud = new VkDocsCloud(new CloudParameters { User = UserId }, new FakeAuthenticator(), services.Configuration.VkDocs, services.Apis.Vk, services.Apis.VkId);

        _ = await Assert.That(async () => await cloud.UploadFile("/", Stream.Null, FileName, CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Covers the constructors that <see cref="VkApiException"/> must offer.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task VkApiException_CarriesItsDetails()
    {
        var inner = new InvalidOperationException("inner");

        await Assert.That(new VkApiException().ErrorCode).IsEqualTo(0);
        await Assert.That(new VkApiException(Message).Message).IsEqualTo(Message);
        await Assert.That(new VkApiException(Message, inner).InnerException).IsSameReferenceAs(inner);
    }

    /// <summary>Creates a signed-in provider over the stubbed clients.</summary>
    /// <param name="services">The stubbed services.</param>
    /// <returns>The provider.</returns>
    private static VkDocsCloud CreateSignedIn(CloudTestServices services)
    {
        var parameters = new CloudParameters { Type = CloudType.VkDocs, Created = TestValues.Moment, Token = TestValues.Token, User = UserId };
        return new(parameters, new FakeAuthenticator(), services.Configuration.VkDocs, services.Apis.Vk, services.Apis.VkId);
    }
}
