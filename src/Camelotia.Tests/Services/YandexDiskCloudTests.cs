// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="YandexDiskCloud"/> against stubbed Yandex Disk and Yandex OAuth replies.</summary>
public sealed class YandexDiskCloudTests
{
    /// <summary>The number of entries the two-page folder holds.</summary>
    private const int EntryCount = 2;

    /// <summary>The disk resource reply for a folder with one entry, out of two.</summary>
    private const string FirstPage = """{"path":"disk:/","type":"dir","name":"disk","size":0,"modified":"2020-01-01T00:00:00+00:00","_embedded":{"total":2,"items":[{"path":"disk:/docs","type":"dir","name":"docs","size":0,"modified":"2020-01-01T00:00:00+00:00"}]}}""";

    /// <summary>The disk resource reply for the second page.</summary>
    private const string SecondPage = """{"path":"disk:/","type":"dir","name":"disk","size":0,"modified":"2020-01-01T00:00:00+00:00","_embedded":{"total":2,"items":[{"path":"disk:/a.txt","type":"file","name":"a.txt","size":3,"modified":"2021-01-01T00:00:00+00:00"}]}}""";

    /// <summary>Lists every page of a folder and maps directories and files.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_ReadsEveryPage()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/v1/disk/resources?path=%2F&limit=200&offset=0", static _ => StubHttpHandler.Json(FirstPage))
            .On(HttpMethod.Get, "/v1/disk/resources?path=%2F&limit=200&offset=1", static _ => StubHttpHandler.Json(SecondPage));
        using var services = new CloudTestServices(handler);
        using var cloud = Create(services, TestValues.Token);

        List<FileModel> files = [.. await cloud.GetFiles("/", CancellationToken.None)];

        await Assert.That(files.Count).IsEqualTo(EntryCount);
        await Assert.That(files[0].IsFolder).IsTrue();
        await Assert.That(files[0].Path).IsEqualTo("/docs");
        await Assert.That(files[1].Name).IsEqualTo("a.txt");
        await Assert.That(files[1].Size).IsEqualTo(TestValues.Content.Length);
        await Assert.That(handler.Requests[0].Headers.Authorization!.ToString()).IsEqualTo($"OAuth {TestValues.Token}");
    }

    /// <summary>Stops listing when a page is empty or the resource has no entries.</summary>
    /// <param name="reply">The disk reply.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("""{"path":"disk:/a","type":"file","name":"a","size":1,"modified":"2020-01-01T00:00:00+00:00"}""")]
    [Arguments("""{"path":"disk:/","type":"dir","name":"disk","size":0,"modified":"2020-01-01T00:00:00+00:00","_embedded":{"total":5,"items":[]}}""")]
    public async Task GetFiles_StopsWhenNoEntriesRemain(string reply)
    {
        var handler = new StubHttpHandler().On(HttpMethod.Get, "/v1/disk/resources", _ => StubHttpHandler.Json(reply));
        using var services = new CloudTestServices(handler);
        using var cloud = Create(services, TestValues.Token);

        var files = await cloud.GetFiles("\\", CancellationToken.None);

        await Assert.That(files).IsEmpty();
        await Assert.That(handler.Requests.Count).IsEqualTo(1);
    }

    /// <summary>Downloads through the one-off link with the same token.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DownloadFile_CopiesTheLinkContent()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/v1/disk/resources/download", static _ => StubHttpHandler.Json("""{"href":"https://downloader.example/file"}"""))
            .On(HttpMethod.Get, "/file", static _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(TestValues.Content) });
        using var services = new CloudTestServices(handler);
        using var cloud = Create(services, TestValues.Token);
        await using var target = new MemoryStream();

        await cloud.DownloadFile("/a.txt", target, CancellationToken.None);

        await Assert.That(target.ToArray()).IsEquivalentTo(TestValues.Content);
        await Assert.That(handler.Requests[1].Headers.Authorization!.ToString()).IsEqualTo($"OAuth {TestValues.Token}");
    }

    /// <summary>Uploads through the one-off link without a token.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task UploadFile_PutsTheContentToTheLink()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/v1/disk/resources/upload?path=%2Fdocs%2Fa.txt", static _ => StubHttpHandler.Json("""{"href":"https://uploader.example/put"}"""))
            .On(HttpMethod.Put, "/put", static _ => new(HttpStatusCode.Created));
        using var services = new CloudTestServices(handler);
        using var cloud = Create(services, TestValues.Token);
        await using var content = new MemoryStream(TestValues.Content);

        await cloud.UploadFile("/docs/", content, "a.txt", CancellationToken.None);

        await Assert.That(handler.Bodies[1]).IsEqualTo("abc");
        await Assert.That(handler.Requests[1].Headers.Authorization).IsNull();
    }

    /// <summary>Creates, renames and deletes through the resource endpoints.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FolderOperations_CallTheResourceEndpoints()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Put, "/v1/disk/resources?path=%2Fdocs%2Fnew", static _ => new(HttpStatusCode.Created))
            .On(HttpMethod.Post, "/v1/disk/resources/move?from=%2Fdocs%2Fa.txt&path=%2Fdocs%2Fb.txt", static _ => new(HttpStatusCode.Created))
            .On(HttpMethod.Delete, "/v1/disk/resources?path=%2Fdocs%2Fb.txt", static _ => new(HttpStatusCode.NoContent));
        using var services = new CloudTestServices(handler);
        using var cloud = Create(services, TestValues.Token);

        await cloud.CreateFolder("/docs", "new", CancellationToken.None);
        await cloud.RenameFile("\\docs\\a.txt", "b.txt", CancellationToken.None);
        await cloud.Delete("/docs/b.txt", isFolder: false, CancellationToken.None);

        await Assert.That(handler.Requests[0].Method).IsEqualTo(HttpMethod.Put);
        await Assert.That(handler.Requests[1].Method).IsEqualTo(HttpMethod.Post);
        await Assert.That(handler.Requests[2].Method).IsEqualTo(HttpMethod.Delete);
    }

    /// <summary>Signs in with the authorization code and PKCE, then signs out.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task OAuth_ExchangesTheCodeForAToken()
    {
        var handler = new StubHttpHandler().On(HttpMethod.Post, "/token", static _ => StubHttpHandler.Json($$"""{"access_token":"{{TestValues.OtherToken}}"}"""));
        using var services = new CloudTestServices(handler);
        var authenticator = new FakeAuthenticator();
        using var cloud = new YandexDiskCloud(new CloudParameters(), authenticator, services.Configuration.YandexDisk, services.Apis.YandexDisk, services.Apis.YandexOAuth);
        var states = new List<bool>();
        using var subscription = cloud.IsAuthorized.Subscribe(states.Add);

        await cloud.OAuth(CancellationToken.None);
        var tokenAfterSignIn = cloud.Parameters.Token;
        await cloud.Logout(CancellationToken.None);

        await Assert.That(states).IsEquivalentTo([false, true, false]);
        await Assert.That(tokenAfterSignIn).IsEqualTo(TestValues.OtherToken);
        await Assert.That(cloud.Parameters.Token).IsNull();
        await Assert.That(handler.Bodies[0]).Contains("grant_type=authorization_code");
        await Assert.That(handler.Bodies[0]).Contains("code_verifier=");
        await Assert.That(authenticator.SignInPages[0].Host).IsEqualTo("oauth.yandex.com");
    }

    /// <summary>Rejects a redirect whose state differs, or that carries no code.</summary>
    /// <param name="query">The redirect query.</param>
    /// <param name="message">The expected failure message.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("code=c&state=other", "The sign-in reply did not match this request.")]
    [Arguments("state={state}&error_description=Denied", "Denied")]
    [Arguments("state={state}", "Yandex did not return a sign-in code.")]
    public async Task OAuth_RejectsABadRedirect(string query, string message)
    {
        using var services = new CloudTestServices(new());
        var authenticator = new FakeAuthenticator { Query = query };
        using var cloud = new YandexDiskCloud(new CloudParameters(), authenticator, services.Configuration.YandexDisk, services.Apis.YandexDisk, services.Apis.YandexOAuth);

        var error = await Assert.That(async () => await cloud.OAuth(CancellationToken.None)).Throws<InvalidOperationException>();

        await Assert.That(error!.Message).IsEqualTo(message);
    }

    /// <summary>Reports its fixed capabilities and refuses unsupported sign-ins.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeYandexDisk()
    {
        using var services = new CloudTestServices(new());
        var parameters = new CloudParameters { Type = CloudType.Yandex, Created = TestValues.Moment, Token = TestValues.Token };
        using var cloud = new YandexDiskCloud(parameters, new FakeAuthenticator(), services.Configuration.YandexDisk, services.Apis.YandexDisk, services.Apis.YandexOAuth);
        var authorized = await cloud.IsAuthorized.FirstAsync();

        await Assert.That(authorized).IsTrue();
        await Assert.That(cloud.Name).IsEqualTo("Yandex");
        await Assert.That(cloud.Id).IsEqualTo(parameters.Id);
        await Assert.That(cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(cloud.Size).IsNull();
        await Assert.That(cloud.InitialPath).IsEqualTo("/");
        await Assert.That(cloud.CanCreateFolder).IsTrue();
        await Assert.That(cloud.SupportsOAuth).IsTrue();
        await Assert.That(cloud.SupportsDirectAuth).IsFalse();
        await Assert.That(cloud.SupportsHostAuth).IsFalse();
        await Assert.That(await cloud.GetBreadCrumbs("/", CancellationToken.None)).IsEmpty();
        _ = await Assert.That(async () => await cloud.DirectAuth(TestValues.Token, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.HostAuth("host", 1, TestValues.Token, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
    }

    /// <summary>Refuses disk calls before the user signs in.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Calls_RequireASignIn()
    {
        using var services = new CloudTestServices(new());
        using var cloud = Create(services, token: null);

        _ = await Assert.That(async () => await cloud.GetFiles("/", CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Creates the provider over the stubbed clients.</summary>
    /// <param name="services">The stubbed services.</param>
    /// <param name="token">The saved token, or <see langword="null"/> for signed out.</param>
    /// <returns>The provider.</returns>
    private static YandexDiskCloud Create(CloudTestServices services, string? token) =>
        new(new CloudParameters { Type = CloudType.Yandex, Token = token }, new FakeAuthenticator(), services.Configuration.YandexDisk, services.Apis.YandexDisk, services.Apis.YandexOAuth);
}
