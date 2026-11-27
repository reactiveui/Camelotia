// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Models;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests <see cref="FileViewModel"/>, <see cref="FolderViewModel"/> and the byte size formatting.</summary>
[System.Diagnostics.DebuggerDisplay("EntryViewModelTests")]
public sealed class EntryViewModelTests : IDisposable
{
    /// <summary>The file name the tests use.</summary>
    private const string FileName = "a.txt";

    /// <summary>The file path the tests use.</summary>
    private const string FilePath = "/a.txt";

    /// <summary>The folder path the tests use.</summary>
    private const string DocsPath = "/docs";

    /// <summary>The size the tests give a file, in bytes.</summary>
    private const long FileSize = 1500;

    /// <summary>The provider that owns the entries.</summary>
    private readonly FakeCloud _cloud = new();

    /// <summary>The provider view model the entries belong to.</summary>
    private readonly CloudViewModel _owner;

    /// <summary>Initializes a new instance of the <see cref="EntryViewModelTests"/> class.</summary>
    public EntryViewModelTests() => _owner = ViewModelBuilder.CreateCloudViewModel(new(), _cloud, new FakeFileManager());

    /// <summary>Shows a file's details, and the fallbacks for an unknown date and size.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FileViewModel_ShowsTheDetails()
    {
        var file = new FileViewModel(_owner, new() { Name = FileName, Path = FilePath, Size = FileSize, Modified = TestValues.Moment });
        var folder = new FileViewModel(_owner, new() { Name = "docs", Path = DocsPath, IsFolder = true });

        await Assert.That((file.Name, file.Path, file.Size, file.IsFile, file.IsFolder)).IsEqualTo((FileName, FilePath, "1.5KB", true, false));
        await Assert.That(file.Modified).Contains("2020");
        await Assert.That(file.Provider).IsSameReferenceAs(_owner);
        await Assert.That((folder.Size, folder.Modified, folder.IsFolder)).IsEqualTo(("Unknown", "A long time ago", true));
    }

    /// <summary>Treats views of the same entry from the same provider as equal.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FileViewModel_ComparesByPathDateAndProvider()
    {
        var model = new FileModel { Name = FileName, Path = FilePath, Modified = TestValues.Moment };
        var first = new FileViewModel(_owner, model);
        var same = new FileViewModel(_owner, new() { Name = "other", Path = FilePath, Modified = TestValues.Moment });
        var otherPath = new FileViewModel(_owner, new() { Name = FileName, Path = "/b.txt", Modified = TestValues.Moment });
        var otherDate = new FileViewModel(_owner, new() { Name = FileName, Path = FilePath });

        await Assert.That(first.Equals(same)).IsTrue();
        await Assert.That(first.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(first.Equals(otherPath)).IsFalse();
        await Assert.That(first.Equals(otherDate)).IsFalse();
        await Assert.That(first.Equals(model)).IsFalse();
    }

    /// <summary>Shows a folder with its subfolders, and compares by name, path and provider.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task FolderViewModel_ShowsAndComparesTheFolder()
    {
        var model = new FolderModel(DocsPath, "docs", [new("/docs/sub", "sub")]);
        var folder = new FolderViewModel(_owner, model);
        var same = new FolderViewModel(_owner, new(DocsPath, "docs"));
        var otherName = new FolderViewModel(_owner, new(DocsPath, "Docs"));
        var otherPath = new FolderViewModel(_owner, new("/other", "docs"));
        List<Camelotia.Presentation.Interfaces.IFolderViewModel> children = [.. folder.Children];

        await Assert.That((folder.Name, folder.FullPath)).IsEqualTo(("docs", DocsPath));
        await Assert.That(children[0].FullPath).IsEqualTo("/docs/sub");
        await Assert.That(folder.Provider).IsSameReferenceAs(_owner);
        await Assert.That(folder.Equals(same)).IsTrue();
        await Assert.That(folder.GetHashCode()).IsEqualTo(same.GetHashCode());
        await Assert.That(folder.Equals(otherName)).IsFalse();
        await Assert.That(folder.Equals(otherPath)).IsFalse();
        await Assert.That(folder.Equals(model)).IsFalse();
        _ = await Assert.That(() => new FolderViewModel(_owner, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Formats byte counts in the largest decimal unit.</summary>
    /// <param name="bytes">The byte count.</param>
    /// <param name="precision">The number of decimal places.</param>
    /// <param name="expected">The expected text.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(0L, 1, "0B")]
    [Arguments(999L, 1, "999B")]
    [Arguments(1_234_567L, 2, "1.23MB")]
    [Arguments(-1_500L, 1, "-1.5KB")]
    public async Task ByteSizeToString_UsesTheLargestUnit(long bytes, int precision, string expected) =>
        await Assert.That(bytes.ByteSizeToString(precision)).IsEqualTo(expected);

    /// <summary>Uses one decimal place by default.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task ByteSizeToString_DefaultsToOneDecimalPlace() =>
        await Assert.That(FileSize.ByteSizeToString()).IsEqualTo("1.5KB");

    /// <inheritdoc/>
    public void Dispose()
    {
        _owner.Dispose();
        _cloud.Dispose();
    }
}
