// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="SftpCloud"/> over a fake SSH.NET client.</summary>
public sealed class SftpCloudTests
{
    /// <summary>The server address the tests sign in to.</summary>
    private const string Host = "sftp.example";

    /// <summary>The server port the tests sign in to.</summary>
    private const int Port = 2222;

    /// <summary>The user name the tests sign in with.</summary>
    private const string User = "user";

    /// <summary>The folder under the root.</summary>
    private const string Docs = "/docs";

    /// <summary>The number of real entries in the root: the dot entries are left out.</summary>
    private const int RootEntryCount = 2;

    /// <summary>Signs in by listing the root, then lists the entries without the dot entries.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_ListsTheRootAndSignsIn()
    {
        var client = CreateClient();
        string? createdFor = null;
        using var cloud = new SftpCloud(new CloudParameters(), (host, port, user, _) =>
        {
            createdFor = $"{host}:{port}:{user}";
            return client.Instance;
        });

        await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None);
        List<FileModel> files = [.. await cloud.GetFiles("\\", CancellationToken.None)];

        await Assert.That(await cloud.IsAuthorized.FirstAsync()).IsTrue();
        await Assert.That(createdFor).IsEqualTo($"{Host}:{Port}:{User}");
        await Assert.That(files.Count).IsEqualTo(RootEntryCount);
        await Assert.That(files[0].IsFolder).IsTrue();
        await Assert.That(files[1].Size).IsEqualTo(TestValues.Content.Length);
    }

    /// <summary>Builds the breadcrumb trail with the subfolders of each folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetBreadCrumbs_ListsEachFolderOnTheWay()
    {
        using var cloud = await SignIn(CreateClient());

        List<FolderModel> trail = [.. await cloud.GetBreadCrumbs("/docs/", CancellationToken.None)];
        List<FolderModel> rootChildren = [.. trail[0].Children];

        await Assert.That(trail.Count).IsEqualTo(RootEntryCount);
        await Assert.That(trail[1].FullPath).IsEqualTo(Docs);
        await Assert.That(rootChildren.Count).IsEqualTo(1);
    }

    /// <summary>Runs each write operation against the server path it names.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Operations_UseTheServerPaths()
    {
        var client = CreateClient();
        using var cloud = await SignIn(client);

        await cloud.CreateFolder("\\docs", "new", CancellationToken.None);
        await cloud.RenameFile("/docs/a.txt", "b.txt", CancellationToken.None);
        await cloud.RenameFile("/a.txt", "c.txt", CancellationToken.None);
        await cloud.Delete("/docs/new", isFolder: true, CancellationToken.None);
        await cloud.Delete("/docs/b.txt", isFolder: false, CancellationToken.None);
        await cloud.UploadFile(Docs, Stream.Null, "u.txt", CancellationToken.None);
        await cloud.DownloadFile("/docs/u.txt", Stream.Null, CancellationToken.None);

        await Assert.That(client.Arguments[client.Calls.IndexOf(nameof(ISftpClient.CreateDirectoryAsync))][0]).IsEqualTo("/docs/new");
        await Assert.That(client.Arguments[client.Calls.IndexOf(nameof(ISftpClient.RenameFileAsync))][1]).IsEqualTo("/docs/b.txt");
        await Assert.That(client.Arguments[client.Calls.LastIndexOf(nameof(ISftpClient.RenameFileAsync))][1]).IsEqualTo("/c.txt");
        await Assert.That(client.Calls).Contains(nameof(ISftpClient.DeleteDirectoryAsync));
        await Assert.That(client.Calls).Contains(nameof(ISftpClient.DeleteFileAsync));
        await Assert.That(client.Arguments[client.Calls.IndexOf(nameof(ISftpClient.UploadFileAsync))][1]).IsEqualTo("/docs/u.txt");
        await Assert.That(client.Calls).Contains(nameof(ISftpClient.DownloadFileAsync));
    }

    /// <summary>Disposes the client when the connection fails.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_DisposesTheClientWhenTheConnectionFails()
    {
        var client = new InterfaceFake<ISftpClient>()
            .On(nameof(ISftpClient.ConnectAsync), static _ => Task.FromException(new IOException("refused")));
        using var cloud = new SftpCloud(new CloudParameters(), (_, _, _, _) => client.Instance);

        _ = await Assert.That(async () => await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None)).Throws<IOException>();

        await Assert.That(client.Calls).Contains(nameof(IDisposable.Dispose));
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

    /// <summary>Connects with SSH.NET by default, which fails fast against a closed local port.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DefaultClient_ConnectsWithSshNet()
    {
        using var cloud = new SftpCloud(new CloudParameters());

        _ = await Assert.That(async () => await cloud.HostAuth("127.0.0.1", 1, User, TestValues.Password, CancellationToken.None)).Throws<Exception>();
    }

    /// <summary>Reports its capabilities and refuses the other sign-in methods.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeSftp()
    {
        var parameters = new CloudParameters { Type = CloudType.Sftp, Created = TestValues.Moment };
        using var cloud = new SftpCloud(parameters);

        await Assert.That(cloud.Name).IsEqualTo("Sftp");
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

    /// <summary>Creates a fake client whose root holds dot entries, one folder and one file, and whose folder is empty.</summary>
    /// <returns>The fake client.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static InterfaceFake<ISftpClient> CreateClient() =>
        new InterfaceFake<ISftpClient>().On(nameof(ISftpClient.ListDirectoryAsync), static args =>
            args[0] is "/" ? Entries(Entry(".", "/.", true), Entry("..", "/..", true), Entry("docs", Docs, true), Entry("a.txt", "/a.txt", false)) : Entries());

    /// <summary>Creates a fake directory entry.</summary>
    /// <param name="name">The entry name.</param>
    /// <param name="fullName">The entry path.</param>
    /// <param name="isDirectory">Whether the entry is a folder.</param>
    /// <returns>The entry.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ISftpFile Entry(string name, string fullName, bool isDirectory) =>
        new InterfaceFake<ISftpFile>()
            .On("get_Name", _ => name)
            .On("get_FullName", _ => fullName)
            .On("get_IsDirectory", _ => isDirectory)
            .On("get_LastWriteTimeUtc", static _ => TestValues.Moment.UtcDateTime)
            .On("get_Length", static _ => (long)TestValues.Content.Length)
            .Instance;

    /// <summary>Yields directory entries asynchronously, as SSH.NET does.</summary>
    /// <param name="entries">The entries to yield.</param>
    /// <returns>The same entries, one at a time.</returns>
    private static async IAsyncEnumerable<ISftpFile> Entries(params ISftpFile[] entries)
    {
        foreach (var entry in entries)
        {
            await Task.Yield();
            yield return entry;
        }
    }

    /// <summary>Creates a provider over a fake client and signs in.</summary>
    /// <param name="client">The fake client.</param>
    /// <returns>The signed-in provider.</returns>
    private static async Task<SftpCloud> SignIn(InterfaceFake<ISftpClient> client)
    {
        var cloud = new SftpCloud(new CloudParameters(), (_, _, _, _) => client.Instance);
        await cloud.HostAuth(Host, Port, User, TestValues.Password, CancellationToken.None);
        return cloud;
    }
}
