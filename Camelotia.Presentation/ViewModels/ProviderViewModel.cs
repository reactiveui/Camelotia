using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using DynamicData;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class ProviderViewModel : ReactiveObject, IProviderViewModel, ISupportsActivation
    {
        private readonly ObservableAsPropertyHelper<IEnumerable<FileModel>> _files;
        private readonly ReactiveCommand<Unit, IEnumerable<FileModel>> _refresh;
        private readonly ObservableAsPropertyHelper<bool> _isCurrentPathEmpty;
        private readonly ReactiveCommand<Unit, Unit> _downloadSelectedFile;
        private readonly ReactiveCommand<Unit, Unit> _uploadToCurrentPath;
        private readonly ReactiveCommand<Unit, Unit> _deleteSelectedFile;
        private readonly ObservableAsPropertyHelper<string> _currentPath;
        private readonly ObservableAsPropertyHelper<bool> _hasErrors;
        private readonly ObservableAsPropertyHelper<bool> _isLoading;
        private readonly ObservableAsPropertyHelper<bool> _canLogout;
        private readonly ObservableAsPropertyHelper<bool> _isReady;
        private readonly ReactiveCommand<Unit, string> _back;
        private readonly ReactiveCommand<Unit, string> _open;
        private readonly ReactiveCommand<Unit, Unit> _logout;
        private readonly IProvider _provider;

        public ProviderViewModel(
            IAuthViewModel authViewModel,
            IFileManager fileManager,
            IScheduler currentThread,
            IScheduler mainThread,
            IProvider provider)
        {
            _provider = provider;
            _refresh = ReactiveCommand.CreateFromTask(
                () => provider.Get(CurrentPath),
                outputScheduler: mainThread);
            
            _files = _refresh
                .Select(files => files
                    .OrderByDescending(file => file.IsFolder)
                    .ThenBy(file => file.Name)
                    .ToList())
                .StartWithEmpty()
                .ToProperty(this, x => x.Files, scheduler: currentThread);

            _isLoading = _refresh
                .IsExecuting
                .ToProperty(this, x => x.IsLoading, scheduler: currentThread);
            
            _isReady = _refresh
                .IsExecuting
                .Select(executing => !executing)
                .Skip(1)
                .ToProperty(this, x => x.IsReady, scheduler: currentThread);
            
            var canOpenCurrentPath = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && file.IsFolder)
                .CombineLatest(_refresh.IsExecuting, (folder, busy) => folder && !busy);
            
            _open = ReactiveCommand.Create(
                () => Path.Combine(CurrentPath, SelectedFile.Name),
                canOpenCurrentPath, mainThread);

            var canCurrentPathGoBack = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path.Length > provider.InitialPath.Length)
                .CombineLatest(_refresh.IsExecuting, (valid, busy) => valid && !busy);
            
            _back = ReactiveCommand.Create(
                () => Path.GetDirectoryName(CurrentPath), 
                canCurrentPathGoBack, mainThread);

            _currentPath = _open
                .Merge(_back)
                .DistinctUntilChanged()
                .ToProperty(this, x => x.CurrentPath, provider.InitialPath, scheduler: currentThread);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Select(path => Unit.Default)
                .InvokeCommand(_refresh);

            this.WhenAnyValue(x => x.CurrentPath)
                .Subscribe(path => SelectedFile = null);

            _isCurrentPathEmpty = this
                .WhenAnyValue(x => x.Files)
                .Skip(2)
                .Where(files => files != null)
                .Select(files => !files.Any())
                .ToProperty(this, x => x.IsCurrentPathEmpty, scheduler: currentThread);

            _hasErrors = _refresh
                .ThrownExceptions
                .Select(exception => true)
                .Merge(_refresh.Select(x => false))
                .ToProperty(this, x => x.HasErrors, scheduler: currentThread);

            var canUploadToCurrentPath = this
                .WhenAnyValue(x => x.CurrentPath)
                .Select(path => path != null)
                .DistinctUntilChanged();
                
            _uploadToCurrentPath = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(fileManager.OpenRead)
                    .Where(response => response.Name != null && response.Stream != null)
                    .Select(x => _provider.UploadFile(CurrentPath, x.Stream, x.Name))
                    .SelectMany(task => task.ToObservable()), 
                canUploadToCurrentPath,
                mainThread);

            _uploadToCurrentPath.InvokeCommand(_refresh);

            var canDownloadSelectedFile = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder)
                .DistinctUntilChanged();
                
            _downloadSelectedFile = ReactiveCommand.CreateFromObservable(
                () => Observable
                    .FromAsync(() => fileManager.OpenWrite(SelectedFile.Name))
                    .Where(stream => stream != null)
                    .Select(stream => _provider.DownloadFile(SelectedFile.Path, stream))
                    .SelectMany(task => task.ToObservable()), 
                canDownloadSelectedFile,
                mainThread);
            
            _uploadToCurrentPath
                .ThrownExceptions
                .Merge(_downloadSelectedFile.ThrownExceptions)
                .Merge(_refresh.ThrownExceptions)
                .Subscribe(Console.WriteLine);

            this.WhenAnyValue(x => x.SelectedFile)
                .Where(file => file != null && file.IsFolder)
                .Buffer(2, 1)
                .Select(files => (files.First().Path, files.Last().Path))
                .DistinctUntilChanged()
                .Where(x => x.Item1 == x.Item2)
                .Select(ignore => Unit.Default)
                .InvokeCommand(_open);
            
            var isAuthEnabled = provider.SupportsDirectAuth || provider.SupportsOAuth;
            var canLogout = provider
                .IsAuthorized
                .Select(loggedIn => loggedIn && isAuthEnabled)
                .DistinctUntilChanged()
                .ObserveOn(mainThread);

            _logout = ReactiveCommand.CreateFromTask(provider.Logout, canLogout);
            _canLogout = canLogout
                .ToProperty(this, x => x.CanLogout, scheduler: currentThread);

            var canDeleteSelection = this
                .WhenAnyValue(x => x.SelectedFile)
                .Select(file => file != null && !file.IsFolder);

            _deleteSelectedFile = ReactiveCommand.CreateFromTask(
                () => provider.Delete(SelectedFile),
                canDeleteSelection);

            _deleteSelectedFile.InvokeCommand(Refresh);
            
            Auth = authViewModel;
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                this.WhenAnyValue(x => x.Auth.IsAuthenticated)
                    .Where(authenticated => authenticated)
                    .Select(ignore => Unit.Default)
                    .InvokeCommand(_refresh)
                    .DisposeWith(disposable);
            });
        }
        
        public IAuthViewModel Auth { get; }
        
        public ViewModelActivator Activator { get; }

        [Reactive] public FileModel SelectedFile { get; set; }
        
        public string CurrentPath => _currentPath?.Value ?? _provider.InitialPath;
        
        public ICommand DownloadSelectedFile => _downloadSelectedFile;

        public ICommand UploadToCurrentPath => _uploadToCurrentPath;

        public ICommand DeleteSelectedFile => _deleteSelectedFile;

        public bool IsCurrentPathEmpty => _isCurrentPathEmpty.Value;
        
        public IEnumerable<FileModel> Files => _files.Value;

        public string Description => _provider.Description;
        
        public bool CanLogout => _canLogout.Value;
        
        public bool IsLoading => _isLoading.Value;

        public bool HasErrors => _hasErrors.Value;

        public bool IsReady => _isReady.Value;

        public string Name => _provider.Name;
        
        public string Size => _provider.Size;

        public ICommand Refresh => _refresh;
        
        public ICommand Logout => _logout;

        public ICommand Back => _back;

        public ICommand Open => _open;
    }
}