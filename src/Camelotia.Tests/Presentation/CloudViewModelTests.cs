// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Models;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests <see cref="CloudViewModel"/> over a fake provider.</summary>
[System.Diagnostics.DebuggerDisplay("CloudViewModelTests")]
public sealed class CloudViewModelTests : IDisposable
{
    /// <summary>The folder the provider lists.</summary>
    private const string Docs = "docs";

    /// <summary>The file the provider lists.</summary>
    private const string FileName = "b.txt";

    /// <summary>The size the provider reports, in bytes.</summary>
    private const long CloudSize = 42;

    /// <summary>The automatic refresh countdown, in seconds.</summary>
    private const int RefreshSeconds = 30;

    /// <summary>The path of the listed folder.</summary>
    private const string DocsPath = "/docs";

    /// <summary>A folder the tests move to directly.</summary>
    private const string Elsewhere = "/elsewhere";

    /// <summary>The number of commands that fail without a selection: download and delete.</summary>
    private const int SelectionErrorCount = 2;

    /// <summary>The provider the view model browses.</summary>
    private readonly FakeCloud _cloud = new() { Name = "Fake", InitialPath = "/", Created = TestValues.Moment };

    /// <summary>The local file dialogs.</summary>
    private readonly FakeFileManager _files = new();

    /// <summary>The saved browser state.</summary>
    private readonly CloudState _state = new();

    /// <summary>Initializes a new instance of the <see cref="CloudViewModelTests"/> class.</summary>
    public CloudViewModelTests()
    {
        _cloud.SupportsDirectAuth = true;
        _cloud.Files = static path => path == "/"
            ? [new() { Name = FileName, Path = "/b.txt" }, new() { Name = Docs, Path = DocsPath, IsFolder = true }, new() { Name = "a.txt", Path = "/a.txt" }]
            : [];
        _cloud.BreadCrumbs = static path => path == "/" ? [new("/", "/", [new(DocsPath, Docs)])] : [];
        _cloud.SetAuthorized(true);
    }

    /// <summary>Lists the folder with folders first, then by name.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Refresh_ListsFoldersFirst()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);

        await model.Refresh.RunAsync(RxVoid.Default);
        List<IFileViewModel> files = [.. model.Files];

        await Assert.That(files[0].Name).IsEqualTo(Docs);
        await Assert.That(files[1].Name).IsEqualTo("a.txt");
        await Assert.That(files[2].Name).IsEqualTo(FileName);
        await Assert.That(model.IsCurrentPathEmpty).IsFalse();
        await Assert.That(model.IsReady).IsTrue();
        await Assert.That(model.HasErrorMessage).IsFalse();
    }

    /// <summary>Keeps the listing when a refresh returns the same entries, and reports an empty folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Refresh_ReportsAnEmptyFolder()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        await model.Refresh.RunAsync(RxVoid.Default);
        var first = model.Files;

        await model.Refresh.RunAsync(RxVoid.Default);
        var unchanged = model.Files;
        _cloud.Files = static _ => [];
        await model.Refresh.RunAsync(RxVoid.Default);

        await Assert.That(unchanged).IsSameReferenceAs(first);
        await Assert.That(model.IsCurrentPathEmpty).IsTrue();
    }

    /// <summary>Reports a failed refresh.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Refresh_ReportsAFailure()
    {
        _cloud.Files = static _ => throw new IOException("offline");
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);

        await model.Refresh.RunAsync(RxVoid.Default);

        await Assert.That(model.HasErrorMessage).IsTrue();
    }

    /// <summary>Opens the selected folder, goes back, and moves to a given folder.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Navigation_ChangesTheCurrentPath()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        await model.Refresh.RunAsync(RxVoid.Default);
        model.SelectedFile = Find(model, Docs);

        var canOpen = model.Open.CanRunNow();
        await model.Open.RunAsync(RxVoid.Default);
        var opened = model.CurrentPath;
        var canGoBack = model.Back.CanRunNow();
        await model.Back.RunAsync(RxVoid.Default);
        var back = model.CurrentPath;
        await model.SetPath.RunAsync(Elsewhere);

        await Assert.That(canOpen).IsTrue();
        await Assert.That(opened).IsEqualTo(DocsPath);
        await Assert.That(canGoBack).IsTrue();
        await Assert.That(back).IsEqualTo("/");
        await Assert.That(model.CurrentPath).IsEqualTo(Elsewhere);
        await Assert.That(_state.CurrentPath).IsEqualTo(Elsewhere);
        await Assert.That(model.SelectedFile).IsNull();
    }

    /// <summary>Goes up one folder in each provider's path format, on every operating system.</summary>
    /// <param name="initialPath">The provider's start folder.</param>
    /// <param name="path">The current folder.</param>
    /// <param name="expected">The folder Back moves to.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments("/", "/docs/sub", DocsPath)]
    [Arguments("/", "/docs/sub/", DocsPath)]
    [Arguments("/", DocsPath, "/")]
    [Arguments("", "repo/folder", "repo")]
    [Arguments("", "repo", "")]
    [Arguments("", "/", "")]
    [Arguments("", "C:\\Users\\me", "C:\\Users")]
    [Arguments("", "C:\\Users", "C:\\")]
    [Arguments("", "C:\\", "")]
    public async Task Back_MovesToTheParentFolder(string initialPath, string path, string expected)
    {
        using var cloud = new FakeCloud { InitialPath = initialPath };
        cloud.SetAuthorized(true);
        using var model = ViewModelBuilder.CreateCloudViewModel(new CloudState { CurrentPath = path }, cloud, _files);

        await model.Back.RunAsync(RxVoid.Default);

        await Assert.That(model.CurrentPath).IsEqualTo(expected);
    }

    /// <summary>Uploads the picked file, and does nothing when the user picks nothing.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Upload_SendsThePickedFile()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);

        await model.UploadToCurrentPath.RunAsync(RxVoid.Default);
        _files.ReadName = string.Empty;
        await model.UploadToCurrentPath.RunAsync(RxVoid.Default);

        await Assert.That(_cloud.Calls).Contains("UploadFile / upload.txt");
        await Assert.That(_cloud.Calls.FindAll(static call => call.StartsWith("UploadFile", StringComparison.Ordinal)).Count).IsEqualTo(1);
    }

    /// <summary>Downloads and deletes the selected file, and refuses both without a selection.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task SelectedFileCommands_ActOnTheSelection()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        var errors = new List<Exception>();
        using var downloadErrors = model.DownloadSelectedFile.ThrownExceptions.Subscribe(errors.Add);
        using var deleteErrors = model.DeleteSelectedFile.ThrownExceptions.Subscribe(errors.Add);
        await model.Refresh.RunAsync(RxVoid.Default);

        await model.DownloadSelectedFile.RunAsync(RxVoid.Default);
        await model.DeleteSelectedFile.RunAsync(RxVoid.Default);
        model.SelectedFile = Find(model, FileName);
        var canDownload = model.DownloadSelectedFile.CanRunNow();
        await model.DownloadSelectedFile.RunAsync(RxVoid.Default);
        await model.DeleteSelectedFile.RunAsync(RxVoid.Default);

        await Assert.That(errors.Count).IsEqualTo(SelectionErrorCount);
        await Assert.That(canDownload).IsTrue();
        await Assert.That(_files.Written).IsEquivalentTo([FileName]);
        await Assert.That(_cloud.Calls).Contains("DownloadFile /b.txt");
        await Assert.That(_cloud.Calls).Contains("Delete /b.txt False");
    }

    /// <summary>Clears the selection.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task UnselectFile_ClearsTheSelection()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        await model.Refresh.RunAsync(RxVoid.Default);
        model.SelectedFile = Find(model, FileName);

        await model.UnselectFile.RunAsync(RxVoid.Default);

        await Assert.That(model.SelectedFile).IsNull();
    }

    /// <summary>Allows sign-out while signed in, and saves the token while authenticated.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Logout_IsAvailableWhileSignedIn()
    {
        _cloud.Parameters = new CloudParameters { Token = TestValues.Token, User = "user" };
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        var savedToken = _state.Token;

        var canLogout = model.CanLogout;
        await model.Logout.RunAsync(RxVoid.Default);
        _cloud.SetAuthorized(false);

        await Assert.That(canLogout).IsTrue();
        await Assert.That(savedToken).IsEqualTo(TestValues.Token);
        await Assert.That(_cloud.Calls).Contains("Logout");
        await Assert.That(_state.Token).IsNull();
        await Assert.That(_state.User).IsNull();
        await Assert.That(model.CanLogout).IsFalse();
    }

    /// <summary>Shows the breadcrumb trail when there is one, and hides it when loading it fails.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task BreadCrumbs_FollowTheProvider()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        await model.Refresh.RunAsync(RxVoid.Default);
        await model.SetPath.RunAsync("/");
        List<IFolderViewModel> trail = [.. model.BreadCrumbs];
        var shown = (model.ShowBreadCrumbs, model.HideBreadCrumbs);

        _cloud.BreadCrumbs = static _ => throw new IOException("offline");
        await model.SetPath.RunAsync(DocsPath);
        await model.Refresh.RunAsync(RxVoid.Default);

        await Assert.That(shown).IsEqualTo((true, false));
        await Assert.That(trail[0].Name).IsEqualTo("/");
        await Assert.That(model.ShowBreadCrumbs).IsFalse();
        await Assert.That(model.HideBreadCrumbs).IsTrue();
    }

    /// <summary>Starts the automatic refresh countdown while activated.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Activation_StartsTheCountdown()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);

        using (model.Activator.Activate())
        {
            await Assert.That(model.RefreshingIn).IsEqualTo(RefreshSeconds);
            model.Folder.IsVisible = true;
            model.Folder.IsVisible = false;
        }

        await Assert.That(_cloud.Calls).Contains("GetFiles /");
    }

    /// <summary>Describes the provider and starts from the saved path.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Properties_DescribeTheProvider()
    {
        _state.CurrentPath = "/saved";
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        var unknownSize = model.Size;
        _cloud.Size = CloudSize;

        await Assert.That(unknownSize).IsEqualTo("Unknown");
        await Assert.That(model.Size).IsEqualTo("42B");
        await Assert.That(model.Name).IsEqualTo("Fake");
        await Assert.That(model.Description).IsEqualTo("Fake file system.");
        await Assert.That(model.Created).IsEqualTo(TestValues.Moment);
        await Assert.That(model.Id).IsEqualTo(_cloud.Id);
        await Assert.That(model.CurrentPath).IsEqualTo("/saved");
        await Assert.That(model.CanInteract).IsTrue();
        await Assert.That(model.IsLoading).IsFalse();
    }

    /// <summary>Rejects missing dependencies.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Constructor_RejectsMissingDependencies()
    {
        using var model = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, _files);
        var factories = new CloudViewModelFactories(
            static owner => owner.Folder,
            static owner => owner.Rename,
            static (file, owner) => new FileViewModel(owner, file),
            static (folder, owner) => new FolderViewModel(owner, folder));

        _ = await Assert.That(() => new CloudViewModel(null!, factories, model.Auth, _files, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new CloudViewModel(_state, null!, model.Auth, _files, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new CloudViewModel(_state, factories, model.Auth, _files, null!)).Throws<ArgumentNullException>();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _cloud.Dispose();

    /// <summary>Finds a listed entry by name.</summary>
    /// <param name="model">The browser view model.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="InvalidOperationException">No entry has the name.</exception>
    private static IFileViewModel Find(CloudViewModel model, string name)
    {
        foreach (var file in model.Files)
        {
            if (file.Name == name)
            {
                return file;
            }
        }

        throw new InvalidOperationException($"{name} is not listed.");
    }
}
