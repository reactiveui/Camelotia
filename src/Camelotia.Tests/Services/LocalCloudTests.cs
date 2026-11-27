// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="LocalCloud"/> in a temporary folder.</summary>
[System.Diagnostics.DebuggerDisplay("LocalCloudTests: {_root.FullName}")]
public sealed class LocalCloudTests : IDisposable
{
    /// <summary>The file name the tests create first.</summary>
    private const string FileName = "a.txt";

    /// <summary>The file name the tests rename to.</summary>
    private const string RenamedName = "b.txt";

    /// <summary>The temporary folder each test works in.</summary>
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("camelotia-local-");

    /// <summary>The provider under test.</summary>
    private readonly LocalCloud _cloud = new(new CloudParameters { Type = CloudType.Local, Created = TestValues.Moment });

    /// <summary>Lists the folder entries with their kind and size.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_ListsFoldersAndFiles()
    {
        _ = _root.CreateSubdirectory("docs");
        await File.WriteAllBytesAsync(Path.Combine(_root.FullName, FileName), TestValues.Content);

        List<FileModel> files = [.. await _cloud.GetFiles(_root.FullName, CancellationToken.None)];
        files.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));

        await Assert.That(files[0].Name).IsEqualTo(FileName);
        await Assert.That(files[0].Size).IsEqualTo(TestValues.Content.Length);
        await Assert.That(files[1].IsFolder).IsTrue();
        await Assert.That(files[1].Size).IsEqualTo(0);
    }

    /// <summary>Lists the drives for the empty path, and throws for a missing folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetFiles_ListsDrivesAndRejectsMissingFolders()
    {
        List<FileModel> drives = [.. await _cloud.GetFiles(string.Empty, CancellationToken.None)];

        await Assert.That(drives).IsNotEmpty();
        await Assert.That(drives[0].IsFolder).IsTrue();
        await Assert.That(_cloud.Size ?? 0).IsGreaterThan(0);
        _ = await Assert.That(async () => await _cloud.GetFiles(Path.Combine(_root.FullName, "missing"), CancellationToken.None)).Throws<DirectoryNotFoundException>();
    }

    /// <summary>Builds the trail from the drive root, and an empty trail for a missing folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task GetBreadCrumbs_StartsAtTheRoot()
    {
        var docs = _root.CreateSubdirectory("docs");

        List<FolderModel> trail = [.. await _cloud.GetBreadCrumbs(docs.FullName, CancellationToken.None)];
        var missing = await _cloud.GetBreadCrumbs(Path.Combine(_root.FullName, "missing"), CancellationToken.None);

        await Assert.That(trail[^1].FullPath).IsEqualTo(docs.FullName);
        await Assert.That(trail[0].FullPath).IsEqualTo(docs.Root.FullName);
        await Assert.That(missing).IsEmpty();
    }

    /// <summary>Uploads, downloads, renames and deletes files and folders.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Operations_ChangeTheFileSystem()
    {
        await using (var upload = new MemoryStream(TestValues.Content))
        {
            await _cloud.UploadFile(_root.FullName, upload, FileName, CancellationToken.None);
        }

        var file = Path.Combine(_root.FullName, FileName);
        await using var target = new MemoryStream();
        await _cloud.DownloadFile(file, target, CancellationToken.None);
        await _cloud.CreateFolder(_root.FullName, "docs", CancellationToken.None);
        await _cloud.RenameFile(Path.Combine(_root.FullName, "docs"), "renamed", CancellationToken.None);
        await _cloud.RenameFile(file, RenamedName, CancellationToken.None);
        await _cloud.Delete(Path.Combine(_root.FullName, "renamed"), isFolder: true, CancellationToken.None);
        await _cloud.Delete(Path.Combine(_root.FullName, RenamedName), isFolder: false, CancellationToken.None);

        await Assert.That(target.ToArray()).IsEquivalentTo(TestValues.Content);
        await Assert.That(_root.EnumerateFileSystemInfos()).IsEmpty();
    }

    /// <summary>Refuses operations that need a folder or a file in the wrong place.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Operations_RejectTheWrongKindOfEntry()
    {
        var file = Path.Combine(_root.FullName, FileName);
        await File.WriteAllBytesAsync(file, TestValues.Content);

        _ = await Assert.That(async () => await _cloud.DownloadFile(_root.FullName, Stream.Null, CancellationToken.None)).Throws<InvalidOperationException>();
        _ = await Assert.That(async () => await _cloud.UploadFile(file, Stream.Null, RenamedName, CancellationToken.None)).Throws<InvalidOperationException>();
        _ = await Assert.That(async () => await _cloud.CreateFolder(file, "docs", CancellationToken.None)).Throws<InvalidOperationException>();
        _ = await Assert.That(async () => await _cloud.RenameFile(_root.Root.FullName, "x", CancellationToken.None)).Throws<InvalidOperationException>();
    }

    /// <summary>Is always signed in and needs no credentials.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeTheLocalFileSystem()
    {
        await _cloud.OAuth(CancellationToken.None);
        await _cloud.Logout(CancellationToken.None);
        await _cloud.DirectAuth("a", TestValues.Password, CancellationToken.None);
        await _cloud.HostAuth("h", 1, "a", TestValues.Password, CancellationToken.None);

        await Assert.That(await _cloud.IsAuthorized.FirstAsync()).IsTrue();
        await Assert.That(_cloud.Name).IsEqualTo("Local");
        await Assert.That(_cloud.Id).IsEqualTo(_cloud.Parameters.Id);
        await Assert.That(_cloud.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(_cloud.InitialPath).IsEmpty();
        await Assert.That(_cloud.CanCreateFolder).IsTrue();
        await Assert.That(_cloud.SupportsDirectAuth).IsFalse();
        await Assert.That(_cloud.SupportsHostAuth).IsFalse();
        await Assert.That(_cloud.SupportsOAuth).IsFalse();
    }

    /// <summary>Lists ready drives, but never an optical drive.</summary>
    /// <param name="type">The drive type.</param>
    /// <param name="isReady">Whether the drive is ready.</param>
    /// <param name="expected">Whether the drive is listed.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(DriveType.Fixed, true, true)]
    [Arguments(DriveType.Fixed, false, false)]
    [Arguments(DriveType.CDRom, true, false)]
    public async Task IsBrowsableDrive_SkipsOpticalAndUnreadyDrives(DriveType type, bool isReady, bool expected) =>
        await Assert.That(LocalCloud.IsBrowsableDrive(type, isReady)).IsEqualTo(expected);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _root.Delete(recursive: true);
}
