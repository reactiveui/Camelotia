// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using System.Runtime.CompilerServices;
using Camelotia.Services.Configuration;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Infrastructure;
using Octokit;
using Octokit.Internal;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="GitHubCloud"/> with the real Octokit client over stubbed GitHub replies.</summary>
public sealed class GitHubCloudTests
{
    /// <summary>The login the tests sign in as.</summary>
    private const string Login = "octocat";

    /// <summary>The repository the tests browse.</summary>
    private const string Repository = "repo";

    /// <summary>The file at the repository root.</summary>
    private const string RepositoryFile = "repo/a.txt";

    /// <summary>The folder at the repository root.</summary>
    private const string RepositoryFolder = "repo/docs";

    /// <summary>The repositories reply.</summary>
    private const string Repositories = """[{"name":"repo","size":5,"updated_at":"2020-01-01T00:00:00Z"}]""";

    /// <summary>The repository root reply: one folder and one file.</summary>
    private const string RootContents = """[{"name":"docs","path":"docs","type":"dir","size":0,"sha":"d"},{"name":"a.txt","path":"a.txt","type":"file","size":3,"sha":"f"}]""";

    /// <summary>The folder reply: one subfolder.</summary>
    private const string DocsContents = """[{"name":"sub","path":"docs/sub","type":"dir","size":0,"sha":"s"}]""";

    /// <summary>The reply to a file change.</summary>
    private const string ChangeSet = """{"content":{"name":"a.txt","path":"a.txt","sha":"n"},"commit":{"sha":"c"}}""";

    /// <summary>The number of entries the repository root holds.</summary>
    private const int RootEntryCount = 2;

    /// <summary>The number of folders on the trail to <c>repo/docs</c>: the root, the repository and the folder.</summary>
    private const int TrailLength = 3;

    /// <summary>Signs in with a token, reading the login from GitHub, then signs out.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DirectAuth_ReadsTheLoginAndLogoutForgetsIt()
    {
        var handler = new StubHttpHandler().On(HttpMethod.Get, "/user", static _ => StubHttpHandler.Json($$"""{"login":"{{Login}}"}"""));
        using var cloud = new GitHubCloud(new CloudParameters(), CreateClient(handler));

        await cloud.DirectAuth("typed", TestValues.Token, CancellationToken.None);
        var signedIn = (cloud.Parameters.User, cloud.Parameters.Token, await cloud.IsAuthorized.FirstAsync());
        await cloud.Logout(CancellationToken.None);

        await Assert.That(signedIn).IsEqualTo((Login, TestValues.Token, true));
        await Assert.That(cloud.Parameters.User).IsNull();
        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
        await Assert.That(handler.Requests[0].Headers.Authorization!.ToString()).IsEqualTo($"Token {TestValues.Token}");
    }

    /// <summary>Lists the repositories at the root, and entries inside a repository.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_ListsRepositoriesAndContents()
    {
        using var cloud = CreateSignedIn(CreateHandler());

        List<FileModel> repositories = [.. await cloud.GetFiles(string.Empty, CancellationToken.None)];
        List<FileModel> files = [.. await cloud.GetFiles("repo\\", CancellationToken.None)];

        await Assert.That(repositories[0].Name).IsEqualTo(Repository);
        await Assert.That(repositories[0].IsFolder).IsTrue();
        await Assert.That(files.Count).IsEqualTo(RootEntryCount);
        await Assert.That(files[0].IsFolder).IsTrue();
        await Assert.That(files[1].Path).IsEqualTo(RepositoryFile);
    }

    /// <summary>Builds the trail from the repository list down to a folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetBreadCrumbs_WalksDownToTheFolder()
    {
        using var cloud = CreateSignedIn(CreateHandler());

        List<FolderModel> rootOnly = [.. await cloud.GetBreadCrumbs(string.Empty, CancellationToken.None)];
        List<FolderModel> trail = [.. await cloud.GetBreadCrumbs(RepositoryFolder, CancellationToken.None)];

        await Assert.That(rootOnly.Count).IsEqualTo(1);
        await Assert.That(trail.Count).IsEqualTo(TrailLength);
        await Assert.That(trail[1].FullPath).IsEqualTo(Repository);
        await Assert.That(trail[2].FullPath).IsEqualTo(RepositoryFolder);
        List<FolderModel> subfolders = [.. trail[2].Children];
        await Assert.That(subfolders[0].FullPath).IsEqualTo("repo/docs/sub");
    }

    /// <summary>Downloads raw content, and uploads and deletes files through the contents API.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FileOperations_UseTheContentsApi()
    {
        var handler = CreateHandler()
            .On(HttpMethod.Put, "/repos/octocat/repo/contents/docs/u.txt", static _ => StubHttpHandler.Json(ChangeSet, HttpStatusCode.Created))
            .On(HttpMethod.Put, "/repos/octocat/repo/contents/v.txt", static _ => StubHttpHandler.Json(ChangeSet, HttpStatusCode.Created))
            .On(HttpMethod.Delete, "/repos/octocat/repo/contents/a.txt", static _ => StubHttpHandler.Json(ChangeSet));
        using var cloud = CreateSignedIn(handler);
        await using var target = new MemoryStream();
        await using var upload = new MemoryStream(TestValues.Content);

        await cloud.DownloadFile(RepositoryFile, target, CancellationToken.None);
        await cloud.UploadFile(RepositoryFolder, upload, "u.txt", CancellationToken.None);
        await cloud.UploadFile(Repository, Stream.Null, "v.txt", CancellationToken.None);
        await cloud.Delete(RepositoryFile, isFolder: false, CancellationToken.None);

        await Assert.That(target.ToArray()).IsEquivalentTo(TestValues.Content);
        await Assert.That(handler.Bodies[handler.Requests.FindIndex(static r => r.Method == HttpMethod.Put)]).Contains(Convert.ToBase64String(TestValues.Content));
        await Assert.That(handler.Bodies[handler.Requests.FindIndex(static r => r.Method == HttpMethod.Delete)]).Contains("\"sha\":\"f\"");
    }

    /// <summary>Refuses what the contents API cannot do.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task UnsupportedOperations_Throw()
    {
        using var cloud = CreateSignedIn(CreateHandler());

        _ = await Assert.That(async () => await cloud.Delete(RepositoryFolder, isFolder: true, CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.CreateFolder(Repository, "new", CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.RenameFile(RepositoryFile, "b.txt", CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.OAuth(CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.HostAuth("host", 1, Login, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
    }

    /// <summary>Starts signed in only with both a saved user and token, and refuses contents calls without a user.</summary>
    /// <param name="user">The saved user.</param>
    /// <param name="token">The saved token.</param>
    /// <param name="expected">Whether the provider starts signed in.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(Login, "saved", true)]
    [Arguments(null, "saved", false)]
    [Arguments(Login, null, false)]
    public async Task Constructor_RestoresTheSavedSignIn(string? user, string? token, bool expected)
    {
        using var cloud = new GitHubCloud(new CloudParameters { User = user, Token = token }, CreateClient(new()));

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsEqualTo(expected);
    }

    /// <summary>Refuses contents calls before a user signs in.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_RequiresASignedInUser()
    {
        using var cloud = new GitHubCloud(new CloudParameters(), CreateClient(new()));

        _ = await Assert.That(async () => await cloud.GetFiles(Repository, CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Reports its capabilities.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeGitHub()
    {
        var parameters = new CloudParameters { Type = CloudType.GitHub, Created = TestValues.Moment };
        using var cloud = new GitHubCloud(parameters, new GitHubCloudOptions());

        await Assert.That(cloud.Name).IsEqualTo("GitHub");
        await Assert.That(cloud.Id).IsEqualTo(parameters.Id);
        await Assert.That(cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(cloud.Size).IsNull();
        await Assert.That(cloud.InitialPath).IsEmpty();
        await Assert.That(cloud.CanCreateFolder).IsFalse();
        await Assert.That(cloud.SupportsDirectAuth).IsTrue();
        await Assert.That(cloud.SupportsHostAuth).IsFalse();
        await Assert.That(cloud.SupportsOAuth).IsFalse();
        _ = await Assert.That(() => new GitHubCloud(parameters, (GitHubCloudOptions)null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Creates a handler that answers the repository and contents reads.</summary>
    /// <returns>The handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StubHttpHandler CreateHandler() =>
        new StubHttpHandler()
            .On(HttpMethod.Get, "/user/repos", static _ => StubHttpHandler.Json(Repositories))
            .On(HttpMethod.Get, "/repos/octocat/repo/contents/docs", static _ => StubHttpHandler.Json(DocsContents))
            .On(HttpMethod.Get, "/repos/octocat/repo/contents/a.txt", static request => request.Headers.Accept.ToString().Contains("raw", StringComparison.Ordinal)
                ? new(HttpStatusCode.OK) { Content = new ByteArrayContent(TestValues.Content) { Headers = { ContentType = new("application/octet-stream") } } }
                : StubHttpHandler.Json("""[{"name":"a.txt","path":"a.txt","type":"file","size":3,"sha":"f"}]"""))
            .On(HttpMethod.Get, "/repos/octocat/repo/contents", static _ => StubHttpHandler.Json(RootContents));

    /// <summary>Creates a real Octokit client that sends every request to the stub.</summary>
    /// <param name="handler">The stub.</param>
    /// <returns>The client.</returns>
    private static GitHubClient CreateClient(StubHttpHandler handler) =>
        new(new Connection(
            new ProductHeaderValue("camelotia-tests"),
            GitHubClient.GitHubApiUrl,
            new InMemoryCredentialStore(Credentials.Anonymous),
            new HttpClientAdapter(() => handler),
            new SimpleJsonSerializer()));

    /// <summary>Creates a provider signed in as <see cref="Login"/> over the stub.</summary>
    /// <param name="handler">The stub.</param>
    /// <returns>The provider.</returns>
    private static GitHubCloud CreateSignedIn(StubHttpHandler handler) =>
        new(new CloudParameters { User = Login, Token = TestValues.Token }, CreateClient(handler));
}
