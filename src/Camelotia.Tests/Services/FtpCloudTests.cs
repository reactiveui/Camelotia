// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Infrastructure;
using FluentFTP;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="FtpCloud"/> over a fake FluentFTP client.</summary>
public sealed class FtpCloudTests
{
    /// <summary>The server address the tests sign in to.</summary>
    private const string Host = "ftp.example";

    /// <summary>The server port the tests sign in to.</summary>
    private const int Port = 2121;

    /// <summary>The user name the tests sign in with.</summary>
    private const string User = "user";

    /// <summary>The number of entries the root listing holds.</summary>
    private const int RootEntryCount = 2;

    /// <summary>The folder under the root.</summary>
    private const string Docs = "/docs";

    /// <summary>Signs in by listing the root, then lists folders and files.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_ListsTheRootAndSignsIn()
    {
        var client = CreateClient();
        string? createdFor = null;
        using var cloud = new FtpCloud(new CloudParameters(), (host, port, user, _) =>
        {
            createdFor = $"{host}:{port}:{user}";
            return client.Instance;
        });

        await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None);
        List<FileModel> files = [.. await cloud.GetFiles("/", CancellationToken.None)];

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsTrue();
        await Assert.That(createdFor).IsEqualTo($"{Host}:{Port}:{User}");
        await Assert.That(files.Count).IsEqualTo(RootEntryCount);
        await Assert.That(files[0].IsFolder).IsTrue();
        await Assert.That(files[0].Modified).IsNull();
        await Assert.That(files[1].Modified).IsEqualTo(TestValues.Moment);
    }

    /// <summary>Builds the breadcrumb trail from the root to the folder, with the subfolders of each.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetBreadCrumbs_ListsEachFolderOnTheWay()
    {
        var client = CreateClient();
        using var cloud = await SignIn(client);

        List<FolderModel> trail = [.. await cloud.GetBreadCrumbs("/docs\\", CancellationToken.None)];

        await Assert.That(trail.Count).IsEqualTo(RootEntryCount);
        await Assert.That(trail[0].FullPath).IsEqualTo("/");
        await Assert.That(trail[1].FullPath).IsEqualTo(Docs);
        await Assert.That(trail[1].Name).IsEqualTo("docs");
        List<FolderModel> rootChildren = [.. trail[0].Children];
        await Assert.That(rootChildren.Count).IsEqualTo(1);
    }

    /// <summary>Runs each write operation against the server path it names.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Operations_UseTheServerPaths()
    {
        var client = CreateClient()
            .On(nameof(IAsyncFtpClient.CreateDirectory), static _ => Task.FromResult(true))
            .On(nameof(IAsyncFtpClient.UploadStream), static _ => Task.FromResult(FtpStatus.Success))
            .On(nameof(IAsyncFtpClient.DownloadStream), static _ => Task.FromResult(true));
        using var cloud = await SignIn(client);

        await cloud.CreateFolder("/docs/", "new", CancellationToken.None);
        await cloud.RenameFile("/docs/a.txt", "b.txt", CancellationToken.None);
        await cloud.RenameFile("/a.txt", "c.txt", CancellationToken.None);
        await cloud.Delete("/docs/new", isFolder: true, CancellationToken.None);
        await cloud.Delete("/docs/b.txt", isFolder: false, CancellationToken.None);
        await cloud.UploadFile(Docs, Stream.Null, "u.txt", CancellationToken.None);
        await cloud.DownloadFile("/docs/u.txt", Stream.Null, CancellationToken.None);

        await Assert.That(ArgumentsOf(client, nameof(IAsyncFtpClient.CreateDirectory))[0]).IsEqualTo("/docs/new");
        await Assert.That(client.Arguments[client.Calls.IndexOf(nameof(IAsyncFtpClient.Rename))][1]).IsEqualTo("/docs/b.txt");
        await Assert.That(ArgumentsOf(client, nameof(IAsyncFtpClient.Rename))[1]).IsEqualTo("/c.txt");
        await Assert.That(client.Calls).Contains(nameof(IAsyncFtpClient.DeleteDirectory));
        await Assert.That(client.Calls).Contains(nameof(IAsyncFtpClient.DeleteFile));
        await Assert.That(ArgumentsOf(client, nameof(IAsyncFtpClient.UploadStream))[1]).IsEqualTo("/docs/u.txt");
    }

    /// <summary>Throws when the server does not send the file.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DownloadFile_ThrowsWhenTheServerSendsNothing()
    {
        var client = CreateClient().On(nameof(IAsyncFtpClient.DownloadStream), static _ => Task.FromResult(false));
        using var cloud = await SignIn(client);

        _ = await Assert.That(async () => await cloud.DownloadFile("/missing", Stream.Null, CancellationToken.None)).Throws<IOException>();
    }

    /// <summary>Disposes the client when the connection fails.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_DisposesTheClientWhenTheConnectionFails()
    {
        var client = new InterfaceFake<IAsyncFtpClient>()
            .On(nameof(IAsyncFtpClient.Connect), static _ => Task.FromException(new IOException("refused")));
        using var cloud = new FtpCloud(new CloudParameters(), (_, _, _, _) => client.Instance);

        _ = await Assert.That(async () => await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None)).Throws<IOException>();

        await Assert.That(client.Calls).Contains(nameof(IAsyncDisposable.DisposeAsync));
        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
    }

    /// <summary>Signs out, then refuses calls until the next sign-in.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Logout_RefusesLaterCalls()
    {
        using var cloud = await SignIn(CreateClient());

        await cloud.Logout(CancellationToken.None);

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsFalse();
        _ = await Assert.That(async () => await cloud.GetFiles("/", CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Reports its capabilities and refuses the other sign-in methods.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeFtp()
    {
        var parameters = new CloudParameters { Type = CloudType.Ftp, Created = TestValues.Moment };
        using var cloud = new FtpCloud(parameters);

        await Assert.That(cloud.Name).IsEqualTo("Ftp");
        await Assert.That(cloud.Id).IsEqualTo(parameters.Id);
        await Assert.That(cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(cloud.Size).IsNull();
        await Assert.That(cloud.InitialPath).IsEqualTo("/");
        await Assert.That(cloud.CanCreateFolder).IsTrue();
        await Assert.That(cloud.SupportsHostAuth).IsTrue();
        await Assert.That(cloud.SupportsOAuth).IsFalse();
        await Assert.That(cloud.SupportsDirectAuth).IsFalse();
        _ = await Assert.That(async () => await cloud.OAuth(CancellationToken.None)).Throws<NotSupportedException>();
        _ = await Assert.That(async () => await cloud.DirectAuth(User, TestValues.Password, CancellationToken.None)).Throws<NotSupportedException>();
    }

    /// <summary>Creates a fake client whose root holds one folder and one file, and whose folder holds nothing.</summary>
    /// <returns>The fake client.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InterfaceFake<IAsyncFtpClient> CreateClient() =>
        new InterfaceFake<IAsyncFtpClient>().On(nameof(IAsyncFtpClient.GetListing), static args =>
            Task.FromResult<FtpListItem[]>(args[0] is "/"
                ? [new("docs", 0, FtpObjectType.Directory, DateTime.MinValue) { FullName = Docs }, new("a.txt", 1, FtpObjectType.File, TestValues.Moment.UtcDateTime) { FullName = "/a.txt" }]
                : []));

    /// <summary>Creates a provider over a fake client and signs in.</summary>
    /// <param name="client">The fake client.</param>
    /// <returns>The signed-in provider.</returns>
    private static async Task<FtpCloud> SignIn(InterfaceFake<IAsyncFtpClient> client)
    {
        var cloud = new FtpCloud(new CloudParameters(), (_, _, _, _) => client.Instance);
        await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None);
        return cloud;
    }

    /// <summary>Gets the arguments of the last call to a method.</summary>
    /// <param name="client">The fake client.</param>
    /// <param name="method">The method name.</param>
    /// <returns>The arguments.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object?[] ArgumentsOf(InterfaceFake<IAsyncFtpClient> client, string method) =>
        client.Arguments[client.Calls.LastIndexOf(method)];
}
