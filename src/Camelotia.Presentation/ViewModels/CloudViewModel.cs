// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.SourceGenerators;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Browses one storage provider: lists a folder, moves between folders and runs file commands.</summary>
[System.Diagnostics.DebuggerDisplay("CloudViewModel: {Name} {CurrentPath}")]
public sealed partial class CloudViewModel : ReactiveObject, ICloudViewModel, IActivatableViewModel, IDisposable
{
    /// <summary>The number of seconds between automatic refreshes.</summary>
    private const int RefreshIntervalSeconds = 30;

    /// <summary>The separators a provider path can use: '/' for remote providers and '\' for the local file system on Windows.</summary>
    private static readonly SearchValues<char> PathSeparators = SearchValues.Create("/\\");

    /// <summary>Holds the subscriptions and helpers that live as long as the view model.</summary>
    private readonly MultipleDisposable _disposables = new();

    /// <summary>The provider this view model browses.</summary>
    private readonly ICloud _cloud;

    /// <summary>The path the browser starts at, from saved state or the provider.</summary>
    private readonly string _startPath;

    /// <summary>Backs <see cref="Files"/>.</summary>
    private ObservableAsPropertyHelper<IEnumerable<IFileViewModel>> _files;

    /// <summary>Backs <see cref="BreadCrumbs"/>.</summary>
    private ObservableAsPropertyHelper<IEnumerable<IFolderViewModel>> _breadCrumbs;

    /// <summary>Backs <see cref="IsCurrentPathEmpty"/>.</summary>
    private ObservableAsPropertyHelper<bool> _isCurrentPathEmpty;

    /// <summary>Backs <see cref="ShowBreadCrumbs"/>.</summary>
    private ObservableAsPropertyHelper<bool> _showBreadCrumbs;

    /// <summary>Backs <see cref="HasErrorMessage"/>.</summary>
    private ObservableAsPropertyHelper<bool> _hasErrorMessage;

    /// <summary>Backs <see cref="HideBreadCrumbs"/>.</summary>
    private ObservableAsPropertyHelper<bool> _hideBreadCrumbs;

    /// <summary>Backs <see cref="IsLoading"/>.</summary>
    private ObservableAsPropertyHelper<bool> _isLoading;

    /// <summary>Backs <see cref="CanLogout"/>.</summary>
    private ObservableAsPropertyHelper<bool> _canLogout;

    /// <summary>Backs <see cref="IsReady"/>.</summary>
    private ObservableAsPropertyHelper<bool> _isReady;

    /// <summary>The command that loads the breadcrumb trail.</summary>
    private ReactiveCommand<RxVoid, IEnumerable<FolderModel>> _getBreadCrumbs;

    /// <summary>Initializes a new instance of the <see cref="CloudViewModel"/> class.</summary>
    /// <param name="state">The saved browser state.</param>
    /// <param name="factories">Create the child view models.</param>
    /// <param name="auth">The sign-in view model.</param>
    /// <param name="files">Asks the user for local files.</param>
    /// <param name="cloud">The provider to browse.</param>
    public CloudViewModel(CloudState state, CloudViewModelFactories factories, IAuthViewModel auth, IFileManager files, ICloud cloud)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(factories);
        ArgumentNullException.ThrowIfNull(cloud);

        _cloud = cloud;
        _startPath = string.IsNullOrEmpty(state.CurrentPath) ? cloud.InitialPath : state.CurrentPath;
        Auth = auth;
        Folder = factories.CreateFolder(this);
        Rename = factories.Rename(this);

        var canInteract = this.WhenAnyValue(
            static x => x.Folder.IsVisible,
            static x => x.Rename.IsVisible,
            static (folder, rename) => !folder && !rename);
        _disposables.Add(canInteract.Subscribe(value => CanInteract = value));

        // The commands built below read CurrentPath while they are created, so it starts at the saved path.
        CurrentPath = _startPath;

        CreateNavigation(canInteract, factories.File);
        CreateBreadCrumbs(factories.Folder);
        CreateFileCommands(files, canInteract);
        WireStateAndRefresh(state);
        this.WhenActivated(ActivateAutoRefresh);
    }

    /// <inheritdoc/>
    [Reactive]
    public partial int RefreshingIn { get; private set; }

    /// <inheritdoc/>
    [Reactive]
    public partial IFileViewModel? SelectedFile { get; set; }

    /// <inheritdoc/>
    public bool IsCurrentPathEmpty => _isCurrentPathEmpty.Value;

    /// <inheritdoc/>
    public IEnumerable<IFileViewModel> Files => _files.Value;

    /// <inheritdoc/>
    public IEnumerable<IFolderViewModel> BreadCrumbs => _breadCrumbs.Value;

    /// <inheritdoc/>
    public bool ShowBreadCrumbs => _showBreadCrumbs.Value;

    /// <inheritdoc/>
    public bool HideBreadCrumbs => _hideBreadCrumbs.Value;

    /// <inheritdoc/>
    [Reactive]
    public partial string CurrentPath { get; private set; }

    /// <inheritdoc/>
    public bool CanLogout => _canLogout.Value;

    /// <inheritdoc/>
    public bool IsLoading => _isLoading.Value;

    /// <inheritdoc/>
    public bool HasErrorMessage => _hasErrorMessage.Value;

    /// <inheritdoc/>
    public bool IsReady => _isReady.Value;

    /// <inheritdoc/>
    [Reactive]
    public partial bool CanInteract { get; private set; }

    /// <inheritdoc/>
    public IAuthViewModel Auth { get; }

    /// <inheritdoc/>
    public IRenameFileViewModel Rename { get; }

    /// <inheritdoc/>
    public ICreateFolderViewModel Folder { get; }

    /// <inheritdoc/>
    public ViewModelActivator Activator { get; } = new();

    /// <inheritdoc/>
    public Guid Id => _cloud.Id;

    /// <inheritdoc/>
    public string Name => _cloud.Name;

    /// <inheritdoc/>
    public DateTimeOffset Created => _cloud.Created;

    /// <inheritdoc/>
    public string Size => _cloud.Size?.ByteSizeToString() ?? "Unknown";

    /// <inheritdoc/>
    public string Description => $"{_cloud.Name} file system.";

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> DownloadSelectedFile { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> UploadToCurrentPath { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> DeleteSelectedFile { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> UnselectFile { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, IEnumerable<FileModel>> Refresh { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Logout { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, string> Back { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, string> Open { get; private set; }

    /// <inheritdoc/>
    public ReactiveCommand<string, string> SetPath { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposables.Dispose();
        _files.Dispose();
        _breadCrumbs.Dispose();
        _isCurrentPathEmpty.Dispose();
        _showBreadCrumbs.Dispose();
        _hasErrorMessage.Dispose();
        _hideBreadCrumbs.Dispose();
        _isLoading.Dispose();
        _canLogout.Dispose();
        _isReady.Dispose();
        _getBreadCrumbs.Dispose();
        Refresh.Dispose();
        Open.Dispose();
        Back.Dispose();
        SetPath.Dispose();
        UploadToCurrentPath.Dispose();
        DownloadSelectedFile.Dispose();
        DeleteSelectedFile.Dispose();
        UnselectFile.Dispose();
        Logout.Dispose();
        Activator.Dispose();
        Folder.Dispose();
        Rename.Dispose();
        Auth.Dispose();
    }

    /// <summary>Checks whether a sequence has any items.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The sequence.</param>
    /// <returns><see langword="true"/> when the sequence has an item.</returns>
    private static bool HasItems<T>(IEnumerable<T> items)
    {
        using var enumerator = items.GetEnumerator();
        return enumerator.MoveNext();
    }

    /// <summary>Gets the folder above a provider path.</summary>
    /// <param name="path">The provider path. Remote providers use '/'; the local file system on Windows uses '\'.</param>
    /// <param name="initialPath">The provider's start folder, used when the path has no parent.</param>
    /// <returns>The parent folder.</returns>
    /// <remarks>
    /// <see cref="Path.GetDirectoryName(string)"/> is not used because it rewrites '/' to '\' on Windows,
    /// which breaks the paths of every remote provider.
    /// </remarks>
    private static string GetParentPath(string path, string initialPath)
    {
        var trimmed = path.TrimEnd('/', '\\');
        var separator = trimmed.AsSpan().LastIndexOfAny(PathSeparators);
        if (separator < 0)
        {
            return initialPath;
        }

        if (separator == 0)
        {
            return path[..1];
        }

        var parent = trimmed[..separator];

        // A Windows drive root keeps its separator, so "C:\Users" goes back to "C:\" and not to "C:".
        return parent.EndsWith(':') ? trimmed[..(separator + 1)] : parent;
    }

    /// <summary>Checks whether two listings hold equal entries in the same order.</summary>
    /// <param name="left">The first listing.</param>
    /// <param name="right">The second listing.</param>
    /// <returns><see langword="true"/> when the listings match.</returns>
    private static bool SameEntries(List<IFileViewModel> left, IEnumerable<IFileViewModel> right)
    {
        var index = 0;
        foreach (var item in right)
        {
            if (index >= left.Count || !left[index].Equals(item))
            {
                return false;
            }

            index++;
        }

        return index == left.Count;
    }

    /// <summary>Orders entries folders first, then by name.</summary>
    /// <param name="left">The first entry.</param>
    /// <param name="right">The second entry.</param>
    /// <returns>A negative number when <paramref name="left"/> comes first, zero when equal, positive otherwise.</returns>
    private static int CompareEntries(IFileViewModel left, IFileViewModel right)
    {
        if (left.IsFolder != right.IsFolder)
        {
            return left.IsFolder ? -1 : 1;
        }

        return string.CompareOrdinal(left.Name, right.Name);
    }

    /// <summary>Creates the entry view models, folders first, then by name.</summary>
    /// <param name="items">The entries the provider listed.</param>
    /// <param name="fileFactory">Creates each entry view model.</param>
    /// <returns>The sorted view models.</returns>
    private List<IFileViewModel> SortFiles(IEnumerable<FileModel> items, FileViewModelFactory fileFactory)
    {
        var files = new List<IFileViewModel>();
        foreach (var item in items)
        {
            files.Add(fileFactory(item, this));
        }

        files.Sort(CompareEntries);
        return files;
    }

    /// <summary>Creates the refresh, open, back and set-path commands, and the properties they drive.</summary>
    /// <param name="canInteract">Signals whether no form is open.</param>
    /// <param name="fileFactory">Creates each entry view model.</param>
    [MemberNotNull(nameof(Refresh), nameof(Open), nameof(Back), nameof(SetPath), nameof(_files), nameof(_isLoading), nameof(_isReady), nameof(_isCurrentPathEmpty), nameof(_hasErrorMessage))]
    private void CreateNavigation(IObservable<bool> canInteract, FileViewModelFactory fileFactory)
    {
        var canRefresh = this.WhenAnyValue(
            static x => x.Folder.IsVisible,
            static x => x.Rename.IsVisible,
            static x => x.Auth.IsAuthenticated,
            static (folder, rename, authenticated) => !folder && !rename && authenticated);
        Refresh = ReactiveCommand.CreateFromTask(cancellationToken => _cloud.GetFiles(CurrentPath, cancellationToken).AsTask(), canRefresh);

        _files = Refresh
            .Select(items => SortFiles(items, fileFactory))
            .Where(items => !SameEntries(items, Files))
            .Select(static items => (IEnumerable<IFileViewModel>)items)
            .ToProperty(this, static x => x.Files, []);
        _isLoading = Refresh.IsExecuting.ToProperty(this, static x => x.IsLoading);
        _isReady = Refresh.IsExecuting.Skip(1).Select(static executing => !executing).ToProperty(this, static x => x.IsReady);
        _isCurrentPathEmpty = this.WhenAnyValue(static x => x.Files).Skip(1).Select(static items => !HasItems(items)).ToProperty(this, static x => x.IsCurrentPathEmpty);
        _hasErrorMessage = Refresh.ThrownExceptions
            .Select(static _ => true)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Merge(Refresh.Select(static _ => false))
            .ToProperty(this, static x => x.HasErrorMessage);

        var canOpen = this.WhenAnyValue(static x => x.SelectedFile)
            .Select(static file => file is { IsFolder: true })
            .CombineLatest(Refresh.IsExecuting, canInteract, static (folder, busy, interact) => folder && interact && !busy);

        // Each provider lists entries with their full path in its own format, so the entry's path is the folder to open.
        Open = ReactiveCommand.Create(() => (SelectedFile ?? throw new InvalidOperationException("Select a folder to open.")).Path, canOpen);

        var canGoBack = this.WhenAnyValue(static x => x.CurrentPath)
            .Select(path => path.Length > _cloud.InitialPath.Length)
            .CombineLatest(Refresh.IsExecuting, canInteract, static (valid, busy, interact) => valid && interact && !busy);
        Back = ReactiveCommand.Create(() => GetParentPath(CurrentPath, _cloud.InitialPath), canGoBack);
        SetPath = ReactiveCommand.Create<string, string>(static path => path);

        _disposables.Add(Open.Merge(Back).Merge(SetPath)
            .DistinctUntilChanged()
            .Log(this, $"Current path changed in {_cloud.Name}")
            .Subscribe(path => CurrentPath = path));
    }

    /// <summary>Creates the breadcrumb command and the properties it drives.</summary>
    /// <param name="folderFactory">Creates each breadcrumb view model.</param>
    [MemberNotNull(nameof(_getBreadCrumbs), nameof(_breadCrumbs), nameof(_showBreadCrumbs), nameof(_hideBreadCrumbs))]
    private void CreateBreadCrumbs(FolderViewModelFactory folderFactory)
    {
        _getBreadCrumbs = ReactiveCommand.CreateFromTask(cancellationToken => _cloud.GetBreadCrumbs(CurrentPath, cancellationToken).AsTask());

        _breadCrumbs = _getBreadCrumbs
            .Where(HasItems)
            .Select(items =>
            {
                var folders = new List<IFolderViewModel>();
                foreach (var folder in items)
                {
                    folders.Add(folderFactory(folder, this));
                }

                return (IEnumerable<IFolderViewModel>)folders;
            })
            .ToProperty(this, static x => x.BreadCrumbs, []);

        _showBreadCrumbs = _getBreadCrumbs.ThrownExceptions
            .Select(static _ => false)
            .Merge(_getBreadCrumbs.Select(HasItems))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .ToProperty(this, static x => x.ShowBreadCrumbs);

        _hideBreadCrumbs = this.WhenAnyValue(static x => x.ShowBreadCrumbs)
            .Select(static show => !show)
            .ToProperty(this, static x => x.HideBreadCrumbs);
    }

    /// <summary>Creates the upload, download, delete, unselect and sign-out commands.</summary>
    /// <param name="files">Asks the user for local files.</param>
    /// <param name="canInteract">Signals whether no form is open.</param>
    [MemberNotNull(nameof(UploadToCurrentPath), nameof(DownloadSelectedFile), nameof(DeleteSelectedFile), nameof(UnselectFile), nameof(Logout), nameof(_canLogout))]
    private void CreateFileCommands(IFileManager files, IObservable<bool> canInteract)
    {
        var selectedFile = this.WhenAnyValue(static x => x.SelectedFile).Select(static file => file is { IsFolder: false });
        var canUseSelectedFile = selectedFile.CombineLatest(Refresh.IsExecuting, canInteract, static (file, loading, interact) => file && !loading && interact);

        UploadToCurrentPath = ReactiveCommand.CreateFromTask(
            cancellationToken => UploadAsync(files, cancellationToken),
            Refresh.IsExecuting.CombineLatest(canInteract, static (loading, interact) => interact && !loading));
        DownloadSelectedFile = ReactiveCommand.CreateFromTask(cancellationToken => DownloadAsync(files, cancellationToken), canUseSelectedFile);
        DeleteSelectedFile = ReactiveCommand.CreateFromTask(DeleteAsync, canUseSelectedFile);

        var canUnselect = this.WhenAnyValue(static x => x.SelectedFile)
            .Select(static selection => selection is not null)
            .CombineLatest(Refresh.IsExecuting, canInteract, static (selected, loading, interact) => selected && !loading && interact);
        UnselectFile = ReactiveCommand.Create(() => { SelectedFile = null; }, canUnselect);

        var canLogout = _cloud.IsAuthorized
            .DistinctUntilChanged()
            .Select(loggedIn => loggedIn && (_cloud.SupportsDirectAuth || _cloud.SupportsOAuth || _cloud.SupportsHostAuth))
            .CombineLatest(canInteract, static (logout, interact) => logout && interact)
            .ObserveOn(RxSchedulers.MainThreadScheduler);
        Logout = ReactiveCommand.CreateFromTask(cancellationToken => _cloud.Logout(cancellationToken).AsTask(), canLogout);
        _canLogout = canLogout.ToProperty(this, static x => x.CanLogout);
    }

    /// <summary>Asks the user for a local file and uploads it to the current folder.</summary>
    /// <param name="files">Asks the user for the file.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>A task that completes when the upload finishes, or at once when the user picks nothing.</returns>
    private async Task UploadAsync(IFileManager files, CancellationToken cancellationToken)
    {
        var (name, stream) = await files.OpenRead(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            await _cloud.UploadFile(CurrentPath, stream, name, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Asks the user where to save the selected file and downloads it there.</summary>
    /// <param name="files">Asks the user for the target.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>A task that completes when the download finishes.</returns>
    /// <exception cref="InvalidOperationException">No file is selected.</exception>
    private async Task DownloadAsync(IFileManager files, CancellationToken cancellationToken)
    {
        var file = SelectedFile ?? throw new InvalidOperationException("Select a file to download.");
        var stream = await files.OpenWrite(file.Name, cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            await _cloud.DownloadFile(file.Path, stream, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Deletes the selected file.</summary>
    /// <param name="cancellationToken">Cancels the delete.</param>
    /// <returns>A task that completes when the file is gone.</returns>
    /// <exception cref="InvalidOperationException">No file is selected.</exception>
    private Task DeleteAsync(CancellationToken cancellationToken)
    {
        var file = SelectedFile ?? throw new InvalidOperationException("Select a file to delete.");
        return _cloud.Delete(file.Path, file.IsFolder, cancellationToken).AsTask();
    }

    /// <summary>Keeps the saved state in step with the browser, and refreshes after each change of folder or file.</summary>
    /// <param name="state">The saved browser state.</param>
    private void WireStateAndRefresh(CloudState state)
    {
        _disposables.Add(this.WhenAnyValue(static x => x.CurrentPath, static x => x.IsReady)
            .Where(static x => x.Property2)
            .Select(static _ => RxVoid.Default)
            .InvokeCommand(_getBreadCrumbs));
        _disposables.Add(this.WhenAnyValue(static x => x.CurrentPath).Skip(1).Select(static _ => RxVoid.Default).InvokeCommand(Refresh));
        _disposables.Add(this.WhenAnyValue(static x => x.CurrentPath).Subscribe(_ => SelectedFile = null));
        _disposables.Add(UploadToCurrentPath.InvokeCommand(Refresh));
        _disposables.Add(DeleteSelectedFile.InvokeCommand(Refresh));

        _disposables.Add(UploadToCurrentPath.ThrownExceptions
            .Merge(DeleteSelectedFile.ThrownExceptions)
            .Merge(DownloadSelectedFile.ThrownExceptions)
            .Merge(Refresh.ThrownExceptions)
            .Merge(_getBreadCrumbs.ThrownExceptions)
            .Log(this, $"Exception occured in provider {_cloud.Name}")
            .Subscribe());

        _disposables.Add(this.WhenAnyValue(static x => x.CurrentPath).Subscribe(path => state.CurrentPath = path));
        _disposables.Add(this.WhenAnyValue(static x => x.Auth.IsAuthenticated).Subscribe(authenticated =>
        {
            state.Token = authenticated ? _cloud.Parameters.Token : null;
            state.User = authenticated ? _cloud.Parameters.User : null;
        }));
    }

    /// <summary>Refreshes on sign-in, every <see cref="RefreshIntervalSeconds"/> seconds, and when a form closes, while the view is active.</summary>
    /// <param name="disposable">Collects the subscriptions; the view's deactivation disposes it.</param>
    private void ActivateAutoRefresh(MultipleDisposable disposable)
    {
        _ = this.WhenAnyValue(static x => x.Auth.IsAuthenticated)
            .Where(static authenticated => authenticated)
            .Select(static _ => RxVoid.Default)
            .InvokeCommand(Refresh)
            .DisposeWith(disposable);

        _ = Signal.After(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Select(_ => RefreshingIn - 1)
            .Where(static value => value >= 0)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(x => RefreshingIn = x)
            .DisposeWith(disposable);

        _ = this.WhenAnyValue(static x => x.RefreshingIn)
            .Skip(1)
            .Where(static refreshing => refreshing == 0)
            .Log(this, $"Refreshing provider {_cloud.Name}")
            .Select(static _ => RxVoid.Default)
            .InvokeCommand(Refresh)
            .DisposeWith(disposable);

        _ = Refresh.Select(static _ => RefreshIntervalSeconds)
            .StartWith(RefreshIntervalSeconds)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(x => RefreshingIn = x)
            .DisposeWith(disposable);

        _ = this.WhenAnyValue(static x => x.CanInteract)
            .Skip(1)
            .Where(static interact => interact)
            .Select(static _ => RxVoid.Default)
            .InvokeCommand(Refresh)
            .DisposeWith(disposable);
    }
}
