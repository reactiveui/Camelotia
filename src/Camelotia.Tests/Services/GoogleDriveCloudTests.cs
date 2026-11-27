// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using Akavache;
using Akavache.SystemTextJson;
using Camelotia.Services.Configuration;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="GoogleDriveCloud"/> with the real Drive client over stubbed Google replies.</summary>
public sealed class GoogleDriveCloudTests
{
    /// <summary>The first page of the file list, which points to a second page.</summary>
    private const string FirstPage = """{"nextPageToken":"p2","files":[{"id":"1","name":"a.txt","size":"3","modifiedTime":"2020-01-01T00:00:00Z"}]}""";

    /// <summary>The last page of the file list.</summary>
    private const string LastPage = """{"files":[{"id":"2","name":"b.txt"}]}""";

    /// <summary>The number of files across both pages.</summary>
    private const int FileCount = 2;

    /// <summary>The path resumable uploads start and continue at.</summary>
    private const string UploadPath = "/upload/drive/v3/files";

    /// <summary>The upload session address the first upload request returns.</summary>
    private const string UploadSession = "https://www.googleapis.com/upload/drive/v3/files?uploadType=resumable&upload_id=u";

    /// <summary>The lifetime of the saved Google token, in seconds.</summary>
    private const int TokenLifetimeSeconds = 3600;

    /// <summary>The longest the restore tests wait for the background sign-in.</summary>
    private static readonly TimeSpan RestoreTimeout = TimeSpan.FromSeconds(10);

    /// <summary>Lists every page of files.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_ReadsEveryPage()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/drive/v3/files?pageSize=1000&pageToken=p2", static _ => StubHttpHandler.Json(LastPage))
            .On(HttpMethod.Get, "/drive/v3/files", static _ => StubHttpHandler.Json(FirstPage));
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var credential = new FakeGoogleCredential();
        using var cloud = Create(cache, handler, credential);

        await cloud.OAuth(CancellationToken.None);
        List<FileModel> files = [.. await cloud.GetFiles("/", CancellationToken.None)];

        await Assert.That(files.Count).IsEqualTo(FileCount);
        await Assert.That(files[0].Size).IsEqualTo(TestValues.Content.Length);
        await Assert.That(files[0].Modified).IsEqualTo(TestValues.Moment);
        await Assert.That(files[1].Size).IsEqualTo(0);
        await Assert.That(credential.InitializedClients).IsGreaterThan(0);
    }

    /// <summary>Downloads, uploads, renames and deletes through the Drive API.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FileOperations_UseTheDriveApi()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/drive/v3/files/1?alt=media", static _ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(TestValues.Content) })
            .On(HttpMethod.Post, UploadPath, static _ => new(HttpStatusCode.OK) { Headers = { Location = new(UploadSession) } })
            .On(HttpMethod.Put, UploadPath, static _ => StubHttpHandler.Json("""{"id":"3"}"""))
            .On(HttpMethod.Patch, "/drive/v3/files/1", static _ => StubHttpHandler.Json("""{"id":"1","name":"b.txt"}"""))
            .On(HttpMethod.Delete, "/drive/v3/files/1", static _ => new(HttpStatusCode.NoContent));
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        using var cloud = Create(cache, handler, new());
        await cloud.OAuth(CancellationToken.None);
        await using var target = new MemoryStream();
        await using var upload = new MemoryStream(TestValues.Content);

        await cloud.DownloadFile("1", target, CancellationToken.None);
        await cloud.UploadFile("/", upload, "c.txt", CancellationToken.None);
        await cloud.RenameFile("1", "b.txt", CancellationToken.None);
        await cloud.Delete("1", isFolder: false, CancellationToken.None);

        await Assert.That(target.ToArray()).IsEquivalentTo(TestValues.Content);
        await Assert.That(handler.Requests[^1].Method).IsEqualTo(HttpMethod.Delete);
    }

    /// <summary>Throws when Drive refuses a download or an upload.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Transfers_ThrowWhenDriveRefuses()
    {
        var handler = new StubHttpHandler()
            .On(HttpMethod.Get, "/drive/v3/files/1?alt=media", static _ => new(HttpStatusCode.InternalServerError))
            .On(HttpMethod.Post, UploadPath, static _ => new(HttpStatusCode.BadRequest));
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        using var cloud = Create(cache, handler, new());
        await cloud.OAuth(CancellationToken.None);

        _ = await Assert.That(async () => await cloud.DownloadFile("1", Stream.Null, CancellationToken.None)).Throws<IOException>();
        _ = await Assert.That(async () => await cloud.UploadFile("/", Stream.Null, "c.txt", CancellationToken.None)).Throws<IOException>();
    }

    /// <summary>Stores the credential through the provider's Akavache store, and signing out removes it.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task OAuth_StoresTheCredentialAndLogoutRemovesIt()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var stored = string.Empty;
        var missing = "unset";
        using var cloud = new GoogleDriveCloud(
            new CloudParameters(),
            cache,
            new GoogleDriveCloudOptions(),
            async (_, user, store, _) =>
            {
                await store.StoreAsync(user, TestValues.Token);
                stored = await store.GetAsync<string>(user);
                missing = await store.GetAsync<string>("missing");
                await store.DeleteAsync<string>("missing");
                await store.ClearAsync();
                return new FakeGoogleCredential();
            },
            new StubGoogleHttpClientFactory(new()));

        await cloud.OAuth(CancellationToken.None);
        var keysWhileSignedIn = await cache.GetAllKeys().ToList().ToTask();
        await cloud.Logout(CancellationToken.None);
        var keysAfterLogout = await cache.GetAllKeys().ToList().ToTask();

        await Assert.That(stored).IsEqualTo(TestValues.Token);
        await Assert.That(missing).IsNull();
        await Assert.That(keysWhileSignedIn.Count).IsEqualTo(1);
        await Assert.That(keysAfterLogout).IsEmpty();
        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
    }

    /// <summary>Signs back in from a saved Google token without opening a browser.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Constructor_RestoresASavedToken()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var parameters = new CloudParameters();
        var options = new GoogleDriveCloudOptions();
        var token = new TokenResponse
        {
            AccessToken = TestValues.Token,
            RefreshToken = TestValues.OtherToken,
            ExpiresInSeconds = TokenLifetimeSeconds,
            IssuedUtc = TimeProvider.System.GetUtcNow().UtcDateTime,
        };
        _ = await cache.InsertObject($"google-drive-{parameters.Id}-{options.GoogleDriveUserName}", token).ToTask();

        using var cloud = new GoogleDriveCloud(parameters, cache, options);
        var authorized = await cloud.IsAuthorized.Where(static value => value).Timeout(RestoreTimeout).FirstAsync();

        await Assert.That(authorized).IsTrue();
    }

    /// <summary>Stays signed out when the saved credential no longer works.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Constructor_StaysSignedOutWhenTheRestoreFails()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var parameters = new CloudParameters();
        _ = await cache.InsertObject($"google-drive-{parameters.Id}-user", TestValues.Token).ToTask();
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using var cloud = new GoogleDriveCloud(
            parameters,
            cache,
            new GoogleDriveCloudOptions(),
            (_, _, _, _) =>
            {
                attempted.SetResult();
                return Task.FromException<IConfigurableHttpClientInitializer>(new HttpRequestException("offline"));
            },
            null);
        await attempted.Task.WaitAsync(RestoreTimeout);

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
    }

    /// <summary>Refuses Drive calls before the user signs in, and reports its capabilities.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeGoogleDrive()
    {
        using var cache = new InMemoryBlobCache(new SystemJsonSerializer());
        var parameters = new CloudParameters { Type = CloudType.GoogleDrive, Created = TestValues.Moment };
        using var cloud = new GoogleDriveCloud(parameters, cache, new GoogleDriveCloudOptions());

        await Assert.That(cloud.Name).IsEqualTo("GoogleDrive");
        await Assert.That(cloud.Id).IsEqualTo(parameters.Id);
        await Assert.That(cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(cloud.Size).IsNull();
        await Assert.That(cloud.InitialPath).IsEqualTo("/");
        await Assert.That(cloud.CanCreateFolder).IsFalse();
        await Assert.That(cloud.SupportsOAuth).IsTrue();
        await Assert.That(cloud.SupportsDirectAuth).IsFalse();
        await Assert.That(cloud.SupportsHostAuth).IsFalse();
        await Assert.That(await cloud.GetBreadCrumbs("/", CancellationToken.None)).IsEmpty();
        _ = await Assert.That(async () => await cloud.GetFiles("/", CancellationToken.None)).Throws<InvalidOperationException>();
        _ = await Assert.That(async () => await cloud.CreateFolder("/", "a", CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.DirectAuth("a", TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.HostAuth("h", 1, "a", TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
    }

    /// <summary>Creates a provider whose sign-in returns the given credential and whose requests go to the stub.</summary>
    /// <param name="cache">The credential cache.</param>
    /// <param name="handler">The stub.</param>
    /// <param name="credential">The credential the sign-in returns.</param>
    /// <returns>The provider.</returns>
    private static GoogleDriveCloud Create(IBlobCache cache, StubHttpHandler handler, FakeGoogleCredential credential) =>
        new(new CloudParameters(), cache, new GoogleDriveCloudOptions(), (_, _, _, _) => Task.FromResult<IConfigurableHttpClientInitializer>(credential), new StubGoogleHttpClientFactory(handler));
}
