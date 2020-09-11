using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IProviderViewModel ProviderViewModelFactory(ProviderState state, IProvider provider);

    public sealed class ProviderViewModel : ReactiveObject, IProviderViewModel, IActivatableViewModel
    {
        private readonly ReactiveCommand<Unit, IEnumerable<FileModel>> _refresh;
        private readonly ReactiveCommand<Unit, IEnumerable<FolderModel>> _getBreadCrumbs;
        private readonly ReactiveCommand<Unit, Unit> _downloadSelectedFile;
        private readonly ReactiveCommand<Unit, Unit> _uploadToCurrentPath;
        private readonly ReactiveCommand<Unit, Unit> _deleteSelectedFile;
        private readonly ReactiveCommand<Unit, Unit> _unselectFile;
        private readonly ReactiveCommand<Unit, string> _back;
        private readonly ReactiveCommand<Unit, string> _open;
        private readonly ReactiveCommand<string, string> _setPath;
        private readonly ReactiveCommand<Unit, Unit> _logout;
        private readonly IProvider _provider;

        public ProviderViewModel(
            ProviderState state,
            CreateFolderViewModelFactory createFolderFactory,
            RenameFileViewModelFactory renameFactory,
            FileViewModelFactory fileFactory,
            FolderViewModelFactory folderFactory,
            IAuthViewModel auth,
            IFileManager files,
            IProvider provider)
        {
            _provider = provider;
            Folder = createFolderFactory(this);
            Rename = renameFactory(this);
            Auth = auth;

            var canInteract = this
                .WhenAnyValue(
                    x => x.Folder.IsVisible,
                    x => x.Rename.IsVisible,
                    (folder, rename) => !folder && !rename);

            canInteract.ToPropertyEx(this, x => x.CanInteract);
            
            _refresh = ReactiveCommand.CreateFromTask(
                () => provider.Get(CurrentPath),
                canInteract);
            
            _refresh.Select(
                    items => items
                        .Select(file => fileFactory(file, this))
                        .OrderByDescending(file => file.IsFolder)
                        .ThenBy(file => file.Name)
                        .ToList())
                .Where(items => Files == null || !items.SequenceEqual(Files))
                .ToPropertyEx(this, x => x.Files);

            _refresh.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            
            _refresh.IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToPropertyEx(this, x => x.IsReady);

            var canOpenCurrentPath = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, canInteract, (folder, busy, ci) => folder && ci && !busy);
            
            _open = ReactiveCommand.Create(
                () => Path.Combine(CurrentPath, SelectedFile.Name),
                canOpenCurrentPath);

            var canCurrentPathGoBack = this
                .WhenAnyValue(x => x.CurrentPath)
                .Where(path => path != null)
                .Select(path => path.Length > provider.InitialPath.Length)
                .CombineLatest(_refresh.IsExecuting, canInteract, (valid, busy, ci) => valid && ci && !busy);
            
            _back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath), 
                canCurrentPathGoBack);

            _setPath = ReactiveCommand.Create<string, string>(path => path);

            _open.Merge(_back)
                .Merge(_setPath)
                .Select(path => path ?? provider.InitialPath)
                .DistinctUntilChanged()
                .Log(this, $"Current path changed in {provider.Name}")
                .ToPropertyEx(this, x => x.CurrentPath, state.CurrentPath ?? provider.InitialPath);

            _getBreadCrumbs = ReactiveCommand.CreateFromTask(
                () => provider.GetBreadCrumbs(CurrentPath)
                );

            _getBreadCrumbs
                .Where(items => items != null && items.Any())
                .Select(items => items.Select(folder => folderFactory(folder, this)))
                .ToPropertyEx(this, x => x.BreadCrumbs);

            _getBreadCrumbs.ThrownExceptions                
                .Select(exception => false)
                .Merge(_getBreadCrumbs.Select(items => items != null && items.Any()))
                .ObserveOn(RxApp.MainThreadScheduler)                
                .ToPropertyEx(this, x => x.ShowBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath, x => x.IsReady)
                .Where(x => x.Item1 != null && x.Item2)
                .Select(_ => Unit.Default)                
                .InvokeCommand(_getBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(path => Unit.Default)
                .InvokeCommand(_refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => SelectedFile = null);

            this.WhenAnyValue(x => x.Files)
                .Skip(1)
                .Where(items => items != null)
                .Select(items => !items.Any())
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
                    .FromAsync(files.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(args => _provider.UploadFile(CurrentPath, args.Stream, args.Name))
                    .SelectMany(task => task.ToObservable()), 
                canUploadToCurrentPath);

            _uploadToCurrentPath.InvokeCommand(_refresh);

            var canDownloadSelectedFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, canInteract, (down, loading, can) => down && !loading && can);
                
            _downloadSelectedFile = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(() => files.OpenWrite(SelectedFile.Name))
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
                .CombineLatest(_refresh.IsExecuting, canInteract, (del, loading, ci) => del && !loading && ci);

            _deleteSelectedFile = ReactiveCommand.CreateFromTask(
                () => provider.Delete(SelectedFile.Path, SelectedFile.IsFolder),
                canDeleteSelection);

            _deleteSelectedFile.InvokeCommand(Refresh);

            var canUnselectFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(selection => selection != null)
                .CombineLatest(_refresh.IsExecuting, canInteract, (sel, loading, ci) => sel && !loading && ci);
            
            _unselectFile = ReactiveCommand.Create(
                () => { SelectedFile = null; },
                canUnselectFile);

            _uploadToCurrentPath.ThrownExceptions
                .Merge(_deleteSelectedFile.ThrownExceptions)
                .Merge(_downloadSelectedFile.ThrownExceptions)
                .Merge(_refresh.ThrownExceptions)
                .Merge(_getBreadCrumbs.ThrownExceptions)
                .Log(this, $"Exception occured in provider {provider.Name}")
                .Subscribe();

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => state.CurrentPath = path);

            this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                .Select(authenticated => authenticated ? _provider.Parameters?.Token : null)
                .Subscribe(token => state.Token = token);

            this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                .Select(authenticated => authenticated ? _provider.Parameters?.User : null)
                .Subscribe(user => state.User = user);
            
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
        public IEnumerable<IFolderViewModel> BreadCrumbs { get; }

        [ObservableAsProperty]
        public bool ShowBreadCrumbs { get; }

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

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

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

        public ICommand SetPath => _setPath;
    }
}