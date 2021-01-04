using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public delegate ICloudViewModel CloudViewModelFactory(CloudState state, ICloud provider);

    public sealed class CloudViewModel : ReactiveObject, ICloudViewModel, IActivatableViewModel
    {
        private readonly ObservableAsPropertyHelper<IEnumerable<IFolderViewModel>> _breadCrumbs;
        private readonly ObservableAsPropertyHelper<IEnumerable<IFileViewModel>> _files;
        private readonly ObservableAsPropertyHelper<bool> _isCurrentPathEmpty;
        private readonly ObservableAsPropertyHelper<bool> _showBreadCrumbs;
        private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
        private readonly ObservableAsPropertyHelper<bool> _hideBreadCrumbs;
        private readonly ObservableAsPropertyHelper<string> _currentPath;
        private readonly ObservableAsPropertyHelper<bool> _canInteract;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _canLogout;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ICloud _cloud;

        public CloudViewModel(
            CloudState state,
            CreateFolderViewModelFactory createFolderFactory,
            RenameFileViewModelFactory renameFactory,
            FileViewModelFactory fileFactory,
            FolderViewModelFactory folderFactory,
            IAuthViewModel auth,
            IFileManager files,
            ICloud cloud)
        {
            _cloud = cloud;
            Folder = createFolderFactory(this);
            Rename = renameFactory(this);
            Auth = auth;

            var canInteract = this
                .WhenAnyValue(
                    x => x.Folder.IsVisible,
                    x => x.Rename.IsVisible,
                    (folder, rename) => !folder && !rename);

            _canInteract = canInteract
                .ToProperty(this, x => x.CanInteract);

            Refresh = ReactiveCommand.CreateFromTask(
                () => cloud.GetFiles(CurrentPath),
                canInteract);

            _files = Refresh
                .Select(
                    items => items
                        .Select(file => fileFactory(file, this))
                        .OrderByDescending(file => file.IsFolder)
                        .ThenBy(file => file.Name)
                        .ToList())
                .Where(items => Files == null || !items.SequenceEqual(Files))
                .ToProperty(this, x => x.Files);

            _isLoading = Refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading);

            _isReady = Refresh
                .IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToProperty(this, x => x.IsReady);

            var canOpenCurrentPath = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && file.IsFolder)
                .CombineLatest(Refresh.IsExecuting, canInteract, (folder, busy, ci) => folder && ci && !busy);

            Open = ReactiveCommand.Create(
                () => Path.Combine(CurrentPath, SelectedFile.Name),
                canOpenCurrentPath);

            var canCurrentPathGoBack = this
                .WhenAnyValue(x => x.CurrentPath)
                .Where(path => path != null)
                .Select(path => path.Length > cloud.InitialPath.Length)
                .CombineLatest(Refresh.IsExecuting, canInteract, (valid, busy, ci) => valid && ci && !busy);

            Back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath),
                canCurrentPathGoBack);

            SetPath = ReactiveCommand.Create<string, string>(path => path);

            _currentPath = Open
                .Merge(Back)
                .Merge(SetPath)
                .Select(path => path ?? cloud.InitialPath)
                .DistinctUntilChanged()
                .Log(this, $"Current path changed in {cloud.Name}")
                .ToProperty(this, x => x.CurrentPath, state.CurrentPath ?? cloud.InitialPath);

            var getBreadCrumbs = ReactiveCommand.CreateFromTask(
                () => cloud.GetBreadCrumbs(CurrentPath));

            _breadCrumbs = getBreadCrumbs
                .Where(items => items != null && items.Any())
                .Select(items => items.Select(folder => folderFactory(folder, this)))
                .ToProperty(this, x => x.BreadCrumbs);

            _showBreadCrumbs = getBreadCrumbs
                .ThrownExceptions
                .Select(exception => false)
                .Merge(getBreadCrumbs.Select(items => items != null && items.Any()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.ShowBreadCrumbs);

            _hideBreadCrumbs = this
                .WhenAnyValue(x => x.ShowBreadCrumbs)
                .Select(show => !show)
                .ToProperty(this, x => x.HideBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath, x => x.IsReady)
                .Where(x => x.Item1 != null && x.Item2)
                .Select(_ => Unit.Default)
                .InvokeCommand(getBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(_ => Unit.Default)
                .InvokeCommand(Refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(_ => SelectedFile = null);

            _isCurrentPathEmpty = this
                .WhenAnyValue(x => x.Files)
                .Skip(1)
                .Where(items => items != null)
                .Select(items => !items.Any())
                .ToProperty(this, x => x.IsCurrentPathEmpty);

            _hasErrorMessage = Refresh
                .ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Merge(Refresh.Select(x => false))
                .ToProperty(this, x => x.HasErrorMessage);

            var canUploadToCurrentPath = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path != null)
                .CombineLatest(Refresh.IsExecuting, canInteract, (up, loading, can) => up && can && !loading);

            UploadToCurrentPath = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(files.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(args => _cloud.UploadFile(CurrentPath, args.Stream, args.Name))
                    .SelectMany(task => task.ToObservable()),
                canUploadToCurrentPath);

            UploadToCurrentPath.InvokeCommand(Refresh);

            var canDownloadSelectedFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(Refresh.IsExecuting, canInteract, (down, loading, can) => down && !loading && can);

            DownloadSelectedFile = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(() => files.OpenWrite(SelectedFile.Name))
                    .Where(stream => stream != null)
                    .Select(stream => _cloud.DownloadFile(SelectedFile.Path, stream))
                    .SelectMany(task => task.ToObservable()),
                canDownloadSelectedFile);

            var canLogout = cloud
                .IsAuthorized
                .DistinctUntilChanged()
                .Select(loggedIn => loggedIn && (
                    cloud.SupportsDirectAuth ||
                    cloud.SupportsOAuth ||
                    cloud.SupportsHostAuth))
                .CombineLatest(canInteract, (logout, interact) => logout && interact)
                .ObserveOn(RxApp.MainThreadScheduler);

            Logout = ReactiveCommand.CreateFromTask(cloud.Logout, canLogout);

            _canLogout = canLogout
                .ToProperty(this, x => x.CanLogout);

            var canDeleteSelection = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(Refresh.IsExecuting, canInteract, (del, loading, ci) => del && !loading && ci);

            DeleteSelectedFile = ReactiveCommand.CreateFromTask(
                () => cloud.Delete(SelectedFile.Path, SelectedFile.IsFolder),
                canDeleteSelection);

            DeleteSelectedFile.InvokeCommand(Refresh);

            var canUnselectFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(selection => selection != null)
                .CombineLatest(Refresh.IsExecuting, canInteract, (sel, loading, ci) => sel && !loading && ci);

            UnselectFile = ReactiveCommand.Create(
                () => { SelectedFile = null; },
                canUnselectFile);

            UploadToCurrentPath.ThrownExceptions
                .Merge(DeleteSelectedFile.ThrownExceptions)
                .Merge(DownloadSelectedFile.ThrownExceptions)
                .Merge(Refresh.ThrownExceptions)
                .Merge(getBreadCrumbs.ThrownExceptions)
                .Log(this, $"Exception occured in provider {cloud.Name}")
                .Subscribe();

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => state.CurrentPath = path);

            this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                .Select(authenticated => authenticated ? _cloud.Parameters?.Token : null)
                .Subscribe(token => state.Token = token);

            this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                .Select(authenticated => authenticated ? _cloud.Parameters?.User : null)
                .Subscribe(user => state.User = user);

            this.WhenActivated(ActivateAutoRefresh);
        }

        [Reactive]
        public int RefreshingIn { get; private set; }

        [Reactive]
        public IFileViewModel SelectedFile { get; set; }

        public bool IsCurrentPathEmpty => _isCurrentPathEmpty.Value;

        public IEnumerable<IFileViewModel> Files => _files.Value;

        public IEnumerable<IFolderViewModel> BreadCrumbs => _breadCrumbs.Value;

        public bool ShowBreadCrumbs => _showBreadCrumbs.Value;

        public bool HideBreadCrumbs => _hideBreadCrumbs.Value;

        public string CurrentPath => _currentPath?.Value;

        public bool CanLogout => _canLogout.Value;

        public bool IsLoading => _isLoading.Value;

        public bool HasErrorMessage => _hasErrorMessage.Value;

        public bool IsReady => _isReady.Value;

        public bool CanInteract => _canInteract?.Value ?? false;

        public IAuthViewModel Auth { get; }

        public IRenameFileViewModel Rename { get; }

        public ICreateFolderViewModel Folder { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public Guid Id => _cloud.Id;

        public string Name => _cloud.Name;

        public DateTime Created => _cloud.Created;

        public string Size => _cloud.Size?.ByteSizeToString() ?? "Unknown";

        public string Description => $"{_cloud.Name} file system.";

        public ReactiveCommand<Unit, Unit> DownloadSelectedFile { get; }

        public ReactiveCommand<Unit, Unit> UploadToCurrentPath { get; }

        public ReactiveCommand<Unit, Unit> DeleteSelectedFile { get; }

        public ReactiveCommand<Unit, Unit> UnselectFile { get; }

        public ReactiveCommand<Unit, IEnumerable<FileModel>> Refresh { get; }

        public ReactiveCommand<Unit, Unit> Logout { get; }

        public ReactiveCommand<Unit, string> Back { get; }

        public ReactiveCommand<Unit, string> Open { get; }

        public ReactiveCommand<string, string> SetPath { get; }

        private void ActivateAutoRefresh(CompositeDisposable disposable)
        {
            this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                .Where(authenticated => authenticated)
                .Select(_ => Unit.Default)
                .InvokeCommand(Refresh)
                .DisposeWith(disposable);

            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                .Select(_ => RefreshingIn - 1)
                .Where(value => value >= 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => RefreshingIn = x)
                .DisposeWith(disposable);

            this.WhenAnyValue(x => x.RefreshingIn)
                .Skip(1)
                .Where(refreshing => refreshing == 0)
                .Log(this, $"Refreshing provider {_cloud.Name} path {CurrentPath}")
                .Select(_ => Unit.Default)
                .InvokeCommand(Refresh)
                .DisposeWith(disposable);

            Refresh.Select(_ => 30)
                .StartWith(30)
                .Subscribe(x => RefreshingIn = x)
                .DisposeWith(disposable);

            this.WhenAnyValue(x => x.CanInteract)
                .Skip(1)
                .Where(interact => interact)
                .Select(x => Unit.Default)
                .InvokeCommand(Refresh)
                .DisposeWith(disposable);
        }
    }
}