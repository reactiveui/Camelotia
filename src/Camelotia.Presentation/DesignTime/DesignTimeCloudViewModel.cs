// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample provider browser for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeCloudViewModel: {Name}")]
public sealed partial class DesignTimeCloudViewModel : ReactiveObject, ICloudViewModel, IDisposable
{
    /// <summary>The sample countdown the designer shows.</summary>
    private const int SampleRefreshSeconds = 30;

    /// <summary>Backs <see cref="Auth"/>.</summary>
    private readonly DesignTimeAuthViewModel _auth = new();

    /// <summary>Backs <see cref="Rename"/>.</summary>
    private readonly DesignTimeRenameFileViewModel _rename = new();

    /// <summary>Backs <see cref="Folder"/>.</summary>
    private readonly DesignTimeCreateFolderViewModel _folder = new();

    /// <summary>Gets the sample provider that parameterless design-time files belong to.</summary>
    public static DesignTimeCloudViewModel Shared { get; } = new();

    /// <inheritdoc/>
    public Guid Id { get; } = new("00000000-0000-0000-0000-000000000001");

    /// <inheritdoc/>
    public IAuthViewModel Auth => _auth;

    /// <inheritdoc/>
    public IRenameFileViewModel Rename => _rename;

    /// <inheritdoc/>
    public ICreateFolderViewModel Folder => _folder;

    /// <inheritdoc/>
    [Reactive]
    public partial IFileViewModel? SelectedFile { get; set; }

    /// <summary>Gets two sample files. They are created on first read, so the constructor does not hand out <see langword="this"/>.</summary>
    public IEnumerable<IFileViewModel> Files => field ??= [new DesignTimeFileViewModel(this), new DesignTimeFileViewModel(this)];

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> DownloadSelectedFile { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> UploadToCurrentPath { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> DeleteSelectedFile { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> UnselectFile { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, IEnumerable<FileModel>> Refresh { get; } = DesignTimeCommands.Returning<IEnumerable<FileModel>>([]);

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Logout { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, string> Back { get; } = DesignTimeCommands.Returning("/home");

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, string> Open { get; } = DesignTimeCommands.Returning("/home/files");

    /// <inheritdoc/>
    public ReactiveCommand<string, string> SetPath { get; } = ReactiveCommand.Create<string, string>(static path => path);

    /// <inheritdoc/>
    public bool IsCurrentPathEmpty => false;

    /// <inheritdoc/>
    public bool IsLoading => false;

    /// <inheritdoc/>
    public bool IsReady => true;

    /// <inheritdoc/>
    public bool HasErrorMessage => false;

    /// <inheritdoc/>
    public bool CanLogout => false;

    /// <inheritdoc/>
    public bool CanInteract => false;

    /// <inheritdoc/>
    public bool ShowBreadCrumbs => true;

    /// <inheritdoc/>
    public bool HideBreadCrumbs => false;

    /// <inheritdoc/>
    public int RefreshingIn => SampleRefreshSeconds;

    /// <inheritdoc/>
    public string CurrentPath => "/home/files";

    /// <inheritdoc/>
    public IEnumerable<IFolderViewModel> BreadCrumbs { get; } =
    [
        new DesignTimeFolderViewModel("home", [new DesignTimeFolderViewModel("home"), new DesignTimeFolderViewModel("home1"), new DesignTimeFolderViewModel("home2")]),
        new DesignTimeFolderViewModel("files", [new DesignTimeFolderViewModel("files"), new DesignTimeFolderViewModel("files1"), new DesignTimeFolderViewModel("files2")]),
    ];

    /// <inheritdoc/>
    public string Description => "Mock file system.";

    /// <inheritdoc/>
    public DateTimeOffset Created { get; } = new(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <inheritdoc/>
    public string Name => "Awesome mock";

    /// <inheritdoc/>
    public string Size => "42MB";

    /// <inheritdoc/>
    public void Dispose()
    {
        _auth.Dispose();
        _rename.Dispose();
        _folder.Dispose();
        DownloadSelectedFile.Dispose();
        UploadToCurrentPath.Dispose();
        DeleteSelectedFile.Dispose();
        UnselectFile.Dispose();
        Refresh.Dispose();
        Logout.Dispose();
        Back.Dispose();
        Open.Dispose();
        SetPath.Dispose();
    }
}
