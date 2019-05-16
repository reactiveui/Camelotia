using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IProviderViewModel ProviderViewModelFactory(IProvider provider, IFileManager files, IAuthViewModel auth);

    public sealed class ProviderViewModel : ReactiveObject, IProviderViewModel, ISupportsActivation
    {
        private readonly ObservableAsPropertyHelper<IEnumerable<IFileViewModel>> _files;
        private readonly ReactiveCommand<Unit, IEnumerable<FileModel>> _refresh;
        private readonly ObservableAsPropertyHelper<bool> _isCurrentPathEmpty;
        private readonly ReactiveCommand<Unit, Unit> _downloadSelectedFile;
        private readonly ReactiveCommand<Unit, Unit> _uploadToCurrentPath;
        private readonly ReactiveCommand<Unit, Unit> _deleteSelectedFile;
        private readonly ObservableAsPropertyHelper<string> _currentPath;
        private readonly ObservableAsPropertyHelper<bool> _canInteract;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _canLogout;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ReactiveCommand<Unit, Unit> _unselectFile;
        private readonly ReactiveCommand<Unit, string> _back;
        private readonly ReactiveCommand<Unit, string> _open;
        private readonly ReactiveCommand<Unit, Unit> _logout;
        private readonly IProvider _provider;

        public ProviderViewModel(
            CreateFolderViewModelFactory createFolder,
            RenameFileViewModelFactory createRename,
            FileViewModelFactory createFile,
            IAuthViewModel authViewModel,
            IFileManager fileManager,
            IProvider provider,
            IScheduler current,
            IScheduler main)
        {
            _provider = provider;
            Folder = createFolder(this);
            Rename = createRename(this);

            var canInteract = this
                .WhenAnyValue(
                    x => x.Folder.IsVisible,
                    x => x.Rename.IsVisible,
                    (folder, rename) => !folder && !rename);

            _canInteract = canInteract
                .DistinctUntilChanged()
                .ToProperty(this, x => x.CanInteract, scheduler: current);
            
            _refresh = ReactiveCommand.CreateFromTask(
                () => provider.Get(CurrentPath),
                canInteract, main);
            
            _files = _refresh
                .Select(files => files
                    .Select(file => createFile(file, this))
                    .OrderByDescending(file => file.IsFolder)
                    .ThenBy(file => file.Name)
                    .ToList())
                .StartWithEmpty()
                .Where(files => Files == null || !files.SequenceEqual(Files))
                .ToProperty(this, x => x.Files, scheduler: current);

            _isLoading = _refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: current);
            
            _isReady = _refresh
                .IsExecuting
                .Select(executing => !executing)
                .Skip(1)
                .ToProperty(this, x => x.IsReady, scheduler: current);
            
            var canOpenCurrentPath = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, (folder, busy) => folder && !busy)
                .CombineLatest(canInteract, (open, interact) => open && interact);
            
            _open = ReactiveCommand.Create(
                () => Path.Combine(CurrentPath, SelectedFile.Name),
                canOpenCurrentPath, main);

            var canCurrentPathGoBack = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path.Length > provider.InitialPath.Length)
                .CombineLatest(_refresh.IsExecuting, (valid, busy) => valid && !busy)
                .CombineLatest(canInteract, (back, interact) => back && interact);
            
            _back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath), 
                canCurrentPathGoBack, main);

            _currentPath = _open
                .Merge(_back)
                .DistinctUntilChanged()
                .Log(this, $"Current path changed in {provider.Name}")
                .ToProperty(this, x => x.CurrentPath, provider.InitialPath, scheduler: current);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(path => Unit.Default)
                .InvokeCommand(_refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => SelectedFile = null);

            _isCurrentPathEmpty = this
                .WhenAnyValue(x => x.Files)
                .Skip(1)
                .Where(files => files != null)
                .Select(files => !files.Any())
                .ToProperty(this, x => x.IsCurrentPathEmpty, scheduler: current);

            _hasErrors = _refresh
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_refresh.Select(x => false))
                .ToProperty(this, x => x.HasErrors, scheduler: current);

            var canUploadToCurrentPath = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path != null)
                .CombineLatest(_refresh.IsExecuting, (up, loading) => up && !loading)
                .CombineLatest(canInteract, (upload, interact) => upload && interact);
                
            _uploadToCurrentPath = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(fileManager.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(x => _provider.UploadFile(CurrentPath, x.Stream, x.Name))
                    .SelectMany(task => task.ToObservable()), 
                canUploadToCurrentPath,
                main);

            _uploadToCurrentPath.InvokeCommand(_refresh);

            var canDownloadSelectedFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, (down, loading) => down && !loading)
                .CombineLatest(canInteract, (download, interact) => download && interact);
                
            _downloadSelectedFile = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(() => fileManager.OpenWrite(SelectedFile.Name))
                    .Where(stream => stream != null)
                    .Select(stream => _provider.DownloadFile(SelectedFile.Path, stream))
                    .SelectMany(task => task.ToObservable()), 
                canDownloadSelectedFile,
                main);
            
            var isAuthEnabled = provider.SupportsDirectAuth || provider.SupportsOAuth;
            var canLogout = provider
                .IsAuthorized
                .Select(loggedIn => loggedIn && isAuthEnabled)
                .DistinctUntilChanged()
                .CombineLatest(canInteract, (logout, interact) => logout && interact)
                .ObserveOn(main);

            _logout = ReactiveCommand.CreateFromTask(provider.Logout, canLogout);
            _canLogout = canLogout
                .ToProperty(this, x => x.CanLogout, scheduler: current);

            var canDeleteSelection = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, (del, loading) => del && !loading)
                .CombineLatest(canInteract, (delete, interact) => delete && interact);

            _deleteSelectedFile = ReactiveCommand.CreateFromTask(
                () => provider.Delete(SelectedFile.Path, SelectedFile.IsFolder),
                canDeleteSelection);

            _deleteSelectedFile.InvokeCommand(Refresh);

            var canUnselectFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(selection => selection != null)
                .CombineLatest(_refresh.IsExecuting, (sel, loading) => sel && !loading)
                .CombineLatest(canInteract, (unselect, interact) => unselect && interact);
            
            _unselectFile = ReactiveCommand.Create(
                () => { SelectedFile = null; },
                canUnselectFile);

            _uploadToCurrentPath
                .ThrownExceptions
                .Merge(_deleteSelectedFile.ThrownExceptions)
                .Merge(_downloadSelectedFile.ThrownExceptions)
                .Merge(_refresh.ThrownExceptions)
                .Log(this, $"Exception occured in provider {provider.Name}")
                .Subscribe();

            Auth = authViewModel;
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                    .Where(authenticated => authenticated)
                    .Select(ignore => Unit.Default)
                    .InvokeCommand(_refresh)
                    .DisposeWith(disposable);

                var interval = TimeSpan.FromSeconds(1);
                Observable.Timer(interval, interval)
                    .Select(unit => RefreshingIn - 1)
                    .Where(value => value >= 0)
                    .ObserveOn(main)
                    .Subscribe(x => RefreshingIn = x)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.RefreshingIn)
                    .Skip(1)
                    .Where(refreshing => refreshing == 0)
                    .Log(this, $"Refreshing provider {provider.Name} path {CurrentPath}")
                    .Select(value => Unit.Default)
                    .InvokeCommand(_refresh)
                    .DisposeWith(disposable);

                const int refreshPeriod = 30;
                _refresh.Select(results => refreshPeriod)
                    .StartWith(refreshPeriod)
                    .Subscribe(x => RefreshingIn = x)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.CanInteract)
                    .Skip(1)
                    .Where(interact => interact)
                    .Select(x => Unit.Default)
                    .InvokeCommand(_refresh);
            });
        }

        public Guid Id => _provider.Id;
        
        public IAuthViewModel Auth { get; }
        
        public IRenameFileViewModel Rename { get; }

        public ViewModelActivator Activator { get; }
        
        public ICreateFolderViewModel Folder { get; }

        [Reactive] public int RefreshingIn { get; private set; }

        [Reactive] public IFileViewModel SelectedFile { get; set; }

        public string Size => _provider.Size?.ByteSizeToString() ?? "Unknown";

        public string CurrentPath => _currentPath?.Value ?? _provider.InitialPath;

        public bool IsCurrentPathEmpty => _isCurrentPathEmpty.Value;
        
        public ICommand DownloadSelectedFile => _downloadSelectedFile;

        public ICommand UploadToCurrentPath => _uploadToCurrentPath;

        public ICommand DeleteSelectedFile => _deleteSelectedFile;
        
        public IEnumerable<IFileViewModel> Files => _files?.Value;

        public bool CanInteract => _canInteract?.Value ?? true;

        public string Description => _provider.Description;
        
        public ICommand UnselectFile => _unselectFile;

        public bool CanLogout => _canLogout.Value;
        
        public bool IsLoading => _isLoading.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsReady => _isReady.Value;

        public string Name => _provider.Name;

        public ICommand Refresh => _refresh;
        
        public ICommand Logout => _logout;

        public ICommand Back => _back;

        public ICommand Open => _open;
    }
}