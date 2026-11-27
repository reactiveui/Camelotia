// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.DesignTime;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests that the design-time view models give the designers complete sample data.</summary>
public sealed class DesignTimeViewModelTests
{
    /// <summary>The number of sample providers and files the samples hold.</summary>
    private const int SampleCount = 2;

    /// <summary>The error message every sample form shows.</summary>
    private const string SampleError = "Error message example.";

    /// <summary>Gives the main window two providers and selects the first.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DesignTimeMainViewModel_HasSampleProviders()
    {
        using var model = new DesignTimeMainViewModel();

        await model.Add.RunAsync(RxVoid.Default);
        await model.Remove.RunAsync(RxVoid.Default);
        await model.Refresh.RunAsync(RxVoid.Default);
        await model.Unselect.RunAsync(RxVoid.Default);

        await Assert.That(model.Clouds.Count).IsEqualTo(SampleCount);
        await Assert.That(model.SelectedProvider).IsSameReferenceAs(model.Clouds[0]);
        await Assert.That(model.SupportedTypes).Contains(CloudType.Sftp);
        await Assert.That(model.SelectedSupportedType).IsEqualTo(CloudType.Sftp);
        await Assert.That((model.WelcomeScreenCollapsed, model.WelcomeScreenVisible, model.IsLoading, model.IsReady)).IsEqualTo((true, false, false, true));
    }

    /// <summary>Gives the provider browser sample files, folders and commands.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DesignTimeCloudViewModel_HasSampleContent()
    {
        using var model = new DesignTimeCloudViewModel();
        List<IFileViewModel> files = [.. model.Files];
        List<IFolderViewModel> trail = [.. model.BreadCrumbs];

        await model.Refresh.RunAsync(RxVoid.Default);
        await model.Back.RunAsync(RxVoid.Default);
        await model.Open.RunAsync(RxVoid.Default);
        await model.SetPath.RunAsync("/a");
        await model.DownloadSelectedFile.RunAsync(RxVoid.Default);
        await model.UploadToCurrentPath.RunAsync(RxVoid.Default);
        await model.DeleteSelectedFile.RunAsync(RxVoid.Default);
        await model.UnselectFile.RunAsync(RxVoid.Default);
        await model.Logout.RunAsync(RxVoid.Default);
        model.SelectedFile = files[0];

        await Assert.That(files.Count).IsEqualTo(SampleCount);
        await Assert.That(model.Files).IsSameReferenceAs(model.Files);
        await Assert.That(files[0].Provider).IsSameReferenceAs(model);
        await Assert.That(trail.Count).IsEqualTo(SampleCount);
        await Assert.That(model.SelectedFile).IsSameReferenceAs(files[0]);
        await Assert.That((model.Name, model.Size, model.Description, model.CurrentPath)).IsEqualTo(("Awesome mock", "42MB", "Mock file system.", "/home/files"));
        await Assert.That((model.IsCurrentPathEmpty, model.IsLoading, model.IsReady, model.HasErrorMessage)).IsEqualTo((false, false, true, false));
        await Assert.That((model.CanLogout, model.CanInteract, model.ShowBreadCrumbs, model.HideBreadCrumbs)).IsEqualTo((false, false, true, false));
        await Assert.That(model.RefreshingIn).IsGreaterThan(0);
        await Assert.That(model.Created.Year).IsEqualTo(TestValues.Moment.Year - 1);
        await Assert.That(model.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(model.Auth).IsTypeOf<DesignTimeAuthViewModel>();
        await Assert.That(model.Rename).IsTypeOf<DesignTimeRenameFileViewModel>();
        await Assert.That(model.Folder).IsTypeOf<DesignTimeCreateFolderViewModel>();
    }

    /// <summary>Gives the sign-in forms sample values that fail validation.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DesignTimeAuthViewModels_HaveSampleForms()
    {
        using var auth = new DesignTimeAuthViewModel();
        var direct = auth.DirectAuth;
        var host = auth.HostAuth;
        var oauth = auth.OAuth;
        direct.Password = TestValues.Password;
        host.Address = "10.0.0.1";
        host.Port = "22";

        await direct.Login.RunAsync(RxVoid.Default);
        await host.Login.RunAsync(RxVoid.Default);
        await oauth.Login.RunAsync(RxVoid.Default);

        await Assert.That((auth.IsAuthenticated, auth.IsAnonymous, auth.SupportsDirectAuth, auth.SupportsHostAuth, auth.SupportsOAuth)).IsEqualTo((true, false, false, false, false));
        await Assert.That((direct.Username, direct.HasErrorMessage, direct.ErrorMessage, direct.IsBusy)).IsEqualTo(("Joseph", true, SampleError, false));
        await Assert.That((host.Username, host.Password, host.Address, host.Port, host.IsBusy)).IsEqualTo(("Jotaro", "Qwerty", "10.0.0.1", "22", false));
        await Assert.That((host.HasErrorMessage, host.ErrorMessage)).IsEqualTo((true, SampleError));
        await Assert.That((oauth.HasErrorMessage, oauth.ErrorMessage, oauth.IsBusy)).IsEqualTo((true, SampleError, false));
        await Assert.That(direct.HasErrors).IsTrue();
    }

    /// <summary>Gives the file forms sample values that fail validation.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DesignTimeFileForms_HaveSampleValues()
    {
        using var folder = new DesignTimeCreateFolderViewModel { Name = "new", IsVisible = true };
        using var rename = new DesignTimeRenameFileViewModel { NewName = "new", IsVisible = true };

        await folder.Create.RunAsync(RxVoid.Default);
        await folder.Open.RunAsync(RxVoid.Default);
        await folder.Close.RunAsync(RxVoid.Default);
        await rename.Rename.RunAsync(RxVoid.Default);
        await rename.Open.RunAsync(RxVoid.Default);
        await rename.Close.RunAsync(RxVoid.Default);

        await Assert.That((folder.Path, folder.IsLoading, folder.IsVisible, folder.HasErrorMessage, folder.ErrorMessage)).IsEqualTo(("/home/path", false, true, true, SampleError));
        await Assert.That((rename.OldName, rename.IsLoading, rename.IsVisible, rename.HasErrorMessage, rename.ErrorMessage)).IsEqualTo(("file", false, true, true, SampleError));
    }

    /// <summary>Gives the entry views sample values.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DesignTimeEntries_HaveSampleValues()
    {
        var file = new DesignTimeFileViewModel();
        var folder = new DesignTimeFolderViewModel();
        var leaf = new DesignTimeFolderViewModel("leaf");
        List<IFolderViewModel> children = [.. folder.Children];

        await Assert.That((file.Name, file.Modified, file.IsFolder, file.IsFile, file.Path, file.Size)).IsEqualTo(("Awesome file.", "2019-01-01 00:00", false, true, "/home/path/file", "42 KB"));
        await Assert.That(file.Provider).IsSameReferenceAs(DesignTimeCloudViewModel.Shared);
        await Assert.That((folder.Name, folder.FullPath)).IsEqualTo(("home", "/home"));
        await Assert.That(folder.Provider).IsSameReferenceAs(DesignTimeCloudViewModel.Shared);
        await Assert.That(children.Count).IsGreaterThan(SampleCount);
        await Assert.That(leaf.Children).IsEmpty();
    }
}
