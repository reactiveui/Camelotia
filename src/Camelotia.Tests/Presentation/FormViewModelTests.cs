// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests <see cref="CreateFolderViewModel"/> and <see cref="RenameFileViewModel"/> inside a real browser view model.</summary>
[System.Diagnostics.DebuggerDisplay("FormViewModelTests")]
public sealed class FormViewModelTests : IDisposable
{
    /// <summary>The name the forms type.</summary>
    private const string NewName = "new";

    /// <summary>The provider the forms act on.</summary>
    private readonly FakeCloud _cloud = new() { InitialPath = "/" };

    /// <summary>The saved browser state.</summary>
    private readonly CloudState _state = new();

    /// <summary>Initializes a new instance of the <see cref="FormViewModelTests"/> class.</summary>
    public FormViewModelTests()
    {
        _cloud.Files = static _ => [new() { Name = "a.txt", Path = "/a.txt" }];
        _cloud.SetAuthorized(true);
    }

    /// <summary>Opens the form, creates the folder in the current path, and closes the form.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task CreateFolder_CreatesAndCloses()
    {
        using var browser = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, new FakeFileManager());
        var form = browser.Folder;

        var canCreateEmpty = form.Create.CanRunNow();
        await form.Open.RunAsync(RxVoid.Default);
        var openedVisible = form.IsVisible;
        form.Name = NewName;
        await form.Create.RunAsync(RxVoid.Default);

        await Assert.That(canCreateEmpty).IsFalse();
        await Assert.That(openedVisible).IsTrue();
        await Assert.That(_cloud.Calls).Contains("CreateFolder / new");
        await Assert.That(form.IsVisible).IsFalse();
        await Assert.That(form.Name).IsEmpty();
        await Assert.That(form.Path).IsEqualTo("/");
        await Assert.That(form.IsLoading).IsFalse();
        await Assert.That(_state.CreateFolderState.IsVisible).IsFalse();
    }

    /// <summary>Reports a failed folder creation, and clears the message when closed.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task CreateFolder_ReportsAFailure()
    {
        _cloud.Operation = static _ => Task.FromException(new IOException("denied"));
        using var browser = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, new FakeFileManager());
        var form = browser.Folder;
        await form.Open.RunAsync(RxVoid.Default);
        form.Name = NewName;

        await form.Create.RunAsync(RxVoid.Default);
        var failure = (form.HasErrorMessage, form.ErrorMessage);
        await form.Close.RunAsync(RxVoid.Default);

        await Assert.That(failure).IsEqualTo((true, "denied"));
        await Assert.That(form.HasErrorMessage).IsFalse();
        await Assert.That(form.ErrorMessage).IsEmpty();
    }

    /// <summary>Refuses to open when the provider cannot create folders, and restores the saved form.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task CreateFolder_FollowsTheProviderAndTheSavedState()
    {
        _cloud.CanCreateFolder = false;
        _state.CreateFolderState.Name = "saved";
        using var browser = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, new FakeFileManager());

        await Assert.That(browser.Folder.Open.CanRunNow()).IsFalse();
        await Assert.That(browser.Folder.Name).IsEqualTo("saved");
        _ = await Assert.That(() => new CreateFolderViewModel(null!, browser, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new CreateFolderViewModel(_state.CreateFolderState, null!, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new CreateFolderViewModel(_state.CreateFolderState, browser, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Renames the selected file and closes the form.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task RenameFile_RenamesTheSelection()
    {
        using var browser = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, new FakeFileManager());
        await browser.Refresh.RunAsync(RxVoid.Default);
        var noSelection = browser.Rename.Open.CanRunNow();
        foreach (var file in browser.Files)
        {
            browser.SelectedFile = file;
        }

        var form = browser.Rename;
        await form.Open.RunAsync(RxVoid.Default);
        form.NewName = NewName;
        await form.Rename.RunAsync(RxVoid.Default);

        await Assert.That(noSelection).IsFalse();
        await Assert.That(form.OldName).IsEqualTo("a.txt");
        await Assert.That(_cloud.Calls).Contains("RenameFile /a.txt new");
        await Assert.That(form.IsVisible).IsFalse();
        await Assert.That(form.NewName).IsEmpty();
        await Assert.That(form.IsLoading).IsFalse();
    }

    /// <summary>Reports a failure, including a rename with no selection.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task RenameFile_ReportsAFailure()
    {
        _state.RenameFileState.NewName = NewName;
        using var browser = ViewModelBuilder.CreateCloudViewModel(_state, _cloud, new FakeFileManager());
        var form = browser.Rename;

        await form.Rename.RunAsync(RxVoid.Default);
        var failure = (form.HasErrorMessage, form.ErrorMessage);
        await form.Close.RunAsync(RxVoid.Default);

        await Assert.That(failure).IsEqualTo((true, "Select a file to rename."));
        await Assert.That(form.HasErrorMessage).IsFalse();
        _ = await Assert.That(() => new RenameFileViewModel(null!, browser, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new RenameFileViewModel(_state.RenameFileState, null!, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new RenameFileViewModel(_state.RenameFileState, browser, null!)).Throws<ArgumentNullException>();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _cloud.Dispose();
}
