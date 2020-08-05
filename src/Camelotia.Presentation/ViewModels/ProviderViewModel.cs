using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Extensions;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IProviderViewModel ProviderViewModelFactory(IProvider provider, IAuthViewModel auth);

    public sealed class ProviderViewModel : ReactiveObject, IProviderViewModel, IActivatableViewModel
    {
        private readonly ReactiveCommand<Unit, IEnumerable<FileModel>> _refresh;
        private readonly ReactiveCommand<Unit, Unit> _downloadSelectedFile;
        private readonly ReactiveCommand<Unit, Unit> _uploadToCurrentPath;
        private readonly ReactiveCommand<Unit, Unit> _deleteSelectedFile;
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
            IProvider provider)
        {
            _provider = provider;
            Folder = createFolder(this);
            Rename = createRename(this);

            var canInteract = this
                .WhenAnyValue(
                    x => x.Folder.IsVisible,
                    x => x.Rename.IsVisible,
                    (folder, rename) => !folder && !rename);

            canInteract.ToPropertyEx(this, x => x.CanInteract);
            
            _refresh = ReactiveCommand.CreateFromTask(
                () => provider.Get(CurrentPath),
                canInteract);
            
            _refresh.Select(files => files
                    .Select(file => createFile(file, this))
                    .OrderByDescending(file => file.IsFolder)
                    .ThenBy(file => file.Name)
                    .ToList())
                .Where(files => Files == null || !files.SequenceEqual(Files))
                .ToPropertyEx(this, x => x.Files);

            _refresh.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            
            _refresh.IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToPropertyEx(this, x => x.IsReady);
            
            var canOpenCurrentPath = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, (folder, busy) => folder && !busy)
                .CombineLatest(canInteract, (open, interact) => open && interact);
            
            _open = ReactiveCommand.Create(
                () => Path.Combine(CurrentPath, SelectedFile.Name),
                canOpenCurrentPath);

            var canCurrentPathGoBack = this
                .WhenAnyValue(x => x.CurrentPath)
                .Where(path => path != null)
                .Select(path => path.Length > provider.InitialPath.Length)
                .CombineLatest(_refresh.IsExecuting, (valid, busy) => valid && !busy)
                .CombineLatest(canInteract, (back, interact) => back && interact);
            
            _back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath), 
                canCurrentPathGoBack);

            _open.Merge(_back)
                .Select(path => path ?? provider.InitialPath)
                .DistinctUntilChanged()
                .Log(this, $"Current path changed in {provider.Name}")
                .ToPropertyEx(this, x => x.CurrentPath, provider.InitialPath);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(path => Unit.Default)
                .InvokeCommand(_refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => SelectedFile = null);

            this.WhenAnyValue(x => x.Files)
                .Skip(1)
                .Where(files => files != null)
                .Select(files => !files.Any())
                .ToPropertyEx(this, x => x.IsCurrentPathEmpty);

            _refresh.ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Merge(_refresh.Select(x => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            var canUploadToCurrentPath = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path != null)
                .CombineLatest(_refresh.IsExecuting, canInteract, (up, loading, can) => up && can && !loading);
                
            _uploadToCurrentPath = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(fileManager.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(x => _provider.UploadFile(CurrentPath, x.Stream, x.Name))
                    .SelectMany(task => task.ToObservable()), 
                canUploadToCurrentPath);

            _uploadToCurrentPath.InvokeCommand(_refresh);

            var canDownloadSelectedFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, canInteract, (down, loading, can) => down && !loading && can);
                
            _downloadSelectedFile = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(() => fileManager.OpenWrite(SelectedFile.Name))
                    .Where(stream => stream != null)
                    .Select(stream => _provider.DownloadFile(SelectedFile.Path, stream))
                    .SelectMany(task => task.ToObservable()), 
                canDownloadSelectedFile);
            
            var canLogout = provider
                .IsAuthorized
                .DistinctUntilChanged()
                .Select(loggedIn => loggedIn && (
                    provider.SupportsDirectAuth ||
                    provider.SupportsOAuth || 
                    provider.SupportsHostAuth))
                .CombineLatest(canInteract, (logout, interact) => logout && interact)
                .ObserveOn(RxApp.MainThreadScheduler);

            _logout = ReactiveCommand.CreateFromTask(provider.Logout, canLogout);
            canLogout.ToPropertyEx(this, x => x.CanLogout);

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

                Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
                    .Select(unit => RefreshingIn - 1)
                    .Where(value => value >= 0)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => RefreshingIn = x)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.RefreshingIn)
                    .Skip(1)
                    .Where(refreshing => refreshing == 0)
                    .Log(this, $"Refreshing provider {provider.Name} path {CurrentPath}")
                    .Select(value => Unit.Default)
                    .InvokeCommand(_refresh)
                    .DisposeWith(disposable);

                _refresh.Select(results => 30)
                    .StartWith(30)
                    .Subscribe(x => RefreshingIn = x)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.CanInteract)
                    .Skip(1)
                    .Where(interact => interact)
                    .Select(x => Unit.Default)
                    .InvokeCommand(_refresh)
                    .DisposeWith(disposable);
            });
        }

        [Reactive]
        public int RefreshingIn { get; private set; }

        [Reactive]
        public IFileViewModel SelectedFile { get; set; }

        [ObservableAsProperty]
        public bool IsCurrentPathEmpty { get; }
        
        [ObservableAsProperty]
        public IEnumerable<IFileViewModel> Files { get; }
        
        [ObservableAsProperty]
        public string CurrentPath { get; }

        [ObservableAsProperty]
        public bool CanLogout { get; }
        
        [ObservableAsProperty]
        public bool IsLoading { get; }

        [ObservableAsProperty]
        public bool HasErrorMessage { get; }

        [ObservableAsProperty]
        public bool IsReady { get; }
        
        [ObservableAsProperty]
        public bool CanInteract { get; }

        public IAuthViewModel Auth { get; }
        
        public IRenameFileViewModel Rename { get; }  
        
        public ICreateFolderViewModel Folder { get; }

        public ViewModelActivator Activator { get; }
        
        public Guid Id => _provider.Id;
        
        public string Name => _provider.Name;
        
        public DateTime Created => _provider.Created;

        public string Size => _provider.Size?.ByteSizeToString() ?? "Unknown";

        public string Description => $"{_provider.Name} file system.";

        public ICommand DownloadSelectedFile => _downloadSelectedFile;

        public ICommand UploadToCurrentPath => _uploadToCurrentPath;

        public ICommand DeleteSelectedFile => _deleteSelectedFile;

        public ICommand UnselectFile => _unselectFile;

        public ICommand Refresh => _refresh;
        
        public ICommand Logout => _logout;

        public ICommand Back => _back;

        public ICommand Open => _open;
    }
}