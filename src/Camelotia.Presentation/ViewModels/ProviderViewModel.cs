using System;
using System.IO;
using System.Linq;
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
            
            Refresh = ReactiveCommand.CreateFromTask(
                () => provider.Get(CurrentPath),
                canInteract);
            
            Refresh.Select(
                    items => items
                        .Select(file => fileFactory(file, this))
                        .OrderByDescending(file => file.IsFolder)
                        .ThenBy(file => file.Name)
                        .ToList())
                .Where(items => Files == null || !items.SequenceEqual(Files))
                .ToPropertyEx(this, x => x.Files);

            Refresh.IsExecuting.ToPropertyEx(this, x => x.IsLoading);
            
            Refresh.IsExecuting
                .Skip(1)
                .Select(executing => !executing)
                .ToPropertyEx(this, x => x.IsReady);

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
                .Select(path => path.Length > provider.InitialPath.Length)
                .CombineLatest(Refresh.IsExecuting, canInteract, (valid, busy, ci) => valid && ci && !busy);
            
            Back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath), 
                canCurrentPathGoBack);

            SetPath = ReactiveCommand.Create<string, string>(path => path);

            Open.Merge(Back)
                .Merge(SetPath)
                .Select(path => path ?? provider.InitialPath)
                .DistinctUntilChanged()
                .Log(this, $"Current path changed in {provider.Name}")
                .ToPropertyEx(this, x => x.CurrentPath, state.CurrentPath ?? provider.InitialPath);

            var getBreadCrumbs = ReactiveCommand.CreateFromTask(
                () => provider.GetBreadCrumbs(CurrentPath));

            getBreadCrumbs
                .Where(items => items != null && items.Any())
                .Select(items => items.Select(folder => folderFactory(folder, this)))
                .ToPropertyEx(this, x => x.BreadCrumbs);

            getBreadCrumbs.ThrownExceptions
                .Select(exception => false)
                .Merge(getBreadCrumbs.Select(items => items != null && items.Any()))
                .ObserveOn(RxApp.MainThreadScheduler)                
                .ToPropertyEx(this, x => x.ShowBreadCrumbs);

            this.WhenAnyValue(x => x.ShowBreadCrumbs)
                .Select(show => !show)
                .ToPropertyEx(this, x => x.HideBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath, x => x.IsReady)
                .Where(x => x.Item1 != null && x.Item2)
                .Select(_ => Unit.Default)                
                .InvokeCommand(getBreadCrumbs);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(path => Unit.Default)
                .InvokeCommand(Refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => SelectedFile = null);

            this.WhenAnyValue(x => x.Files)
                .Skip(1)
                .Where(items => items != null)
                .Select(items => !items.Any())
                .ToPropertyEx(this, x => x.IsCurrentPathEmpty);

            Refresh.ThrownExceptions
                .Select(exception => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Merge(Refresh.Select(x => false))
                .ToPropertyEx(this, x => x.HasErrorMessage);

            var canUploadToCurrentPath = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path != null)
                .CombineLatest(Refresh.IsExecuting, canInteract, (up, loading, can) => up && can && !loading);
                
            UploadToCurrentPath = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(files.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(args => _provider.UploadFile(CurrentPath, args.Stream, args.Name))
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

            Logout = ReactiveCommand.CreateFromTask(provider.Logout, canLogout);
            canLogout.ToPropertyEx(this, x => x.CanLogout);

            var canDeleteSelection = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .CombineLatest(Refresh.IsExecuting, canInteract, (del, loading, ci) => del && !loading && ci);

            DeleteSelectedFile = ReactiveCommand.CreateFromTask(
                () => provider.Delete(SelectedFile.Path, SelectedFile.IsFolder),
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
                    .InvokeCommand(Refresh)
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
                    .InvokeCommand(Refresh)
                    .DisposeWith(disposable);

                Refresh.Select(results => 30)
                    .StartWith(30)
                    .Subscribe(x => RefreshingIn = x)
                    .DisposeWith(disposable);

                this.WhenAnyValue(x => x.CanInteract)
                    .Skip(1)
                    .Where(interact => interact)
                    .Select(x => Unit.Default)
                    .InvokeCommand(Refresh)
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
        public bool HideBreadCrumbs { get; }

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

        public ReactiveCommand<Unit, Unit> DownloadSelectedFile { get; }

        public ReactiveCommand<Unit, Unit> UploadToCurrentPath { get; }

        public ReactiveCommand<Unit, Unit> DeleteSelectedFile { get; }

        public ReactiveCommand<Unit, Unit> UnselectFile { get; }

        public ReactiveCommand<Unit, IEnumerable<FileModel>> Refresh { get; }

        public ReactiveCommand<Unit, Unit> Logout { get; }

        public ReactiveCommand<Unit, string> Back { get; }

        public ReactiveCommand<Unit, string> Open { get; }

        public ReactiveCommand<string, string> SetPath { get; }
    }
}