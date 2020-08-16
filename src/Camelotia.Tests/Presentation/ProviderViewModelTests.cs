using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation
{
    public sealed class ProviderViewModelTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly ICreateFolderViewModel _folder = Substitute.For<ICreateFolderViewModel>();
        private readonly IRenameFileViewModel _rename = Substitute.For<IRenameFileViewModel>();
        private readonly IAuthViewModel _auth = Substitute.For<IAuthViewModel>();
        private readonly IFileManager _files = Substitute.For<IFileManager>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly ProviderState _state = new ProviderState();

        [Fact]
        public void ShouldDisplayLoadingReadyIndicatorsProperly() 
        {
            var model = BuildProviderViewModel();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeFalse();
                
            model.Refresh.Execute(null);
            model.IsReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldDisplayCurrentPathProperly() 
        {
            _provider.InitialPath.Returns(Separator);
            _provider.Get(Separator).ReturnsForAnyArgs(Enumerable.Empty<FileModel>());
            
            var model = BuildProviderViewModel();
            model.IsCurrentPathEmpty.Should().BeFalse();
            model.CurrentPath.Should().Be(Separator);
            model.Files.Should().BeNullOrEmpty();

            model.Refresh.Execute(null);
            model.IsCurrentPathEmpty.Should().BeTrue();
            model.CurrentPath.Should().Be(Separator);
            model.Files.Should().BeEmpty();
        }

        [Fact]
        public void ShouldInheritMetaDataFromProvider() 
        {
            var now = DateTime.Now;
            _provider.Name.Returns("Foo");
            _provider.Size.Returns(42);
            _provider.Created.Returns(now);

            var model = BuildProviderViewModel();
            model.Name.Should().Be("Foo");
            model.Size.Should().Be("42B");
            model.Description.Should().Be("Foo file system.");
            model.Created.Should().Be(now);
        }

        [Fact]
        public void LogoutShouldBeEnabledOnlyWhenAuthorized()
        {
            var authorized = new BehaviorSubject<bool>(true);
            _provider.IsAuthorized.Returns(authorized);
            _provider.SupportsDirectAuth.Returns(true);

            var model = BuildProviderViewModel();
            model.Logout.CanExecute(null).Should().BeTrue();
            model.Logout.Execute(null);
            
            authorized.OnNext(false);
            _provider.Received(1).Logout();
            model.Logout.CanExecute(null).Should().BeFalse();            
        }

        [Fact]
        public void ShouldBeAbleToOpenSelectedPath() 
        {
            var file = new FileModel { Name = "foo", Path = Separator + "foo", IsFolder = true };
            _provider.Get(Separator).Returns(Enumerable.Repeat(file, 1));
            _auth.IsAuthenticated.Returns(true);
            _provider.InitialPath.Returns(Separator);

            var model = BuildProviderViewModel();
            using (model.Activator.Activate())
            {
                model.Files.Should().NotBeEmpty();
                model.CurrentPath.Should().Be(Separator);
                
                model.SelectedFile = model.Files.First();
                model.Open.CanExecute(null).Should().BeTrue();
                model.Open.Execute(null);
                
                model.CurrentPath.Should().Be(Separator + "foo");
                model.Back.CanExecute(null).Should().BeTrue();
                model.Back.Execute(null);
                
                model.CurrentPath.Should().Be(Separator);
            }
        }

        [Fact]
        public void ShouldRefreshContentOfCurrentPathWhenFileIsUploaded() 
        {
            _provider.InitialPath.Returns(Separator);            
            _files.OpenRead().Returns(("example", Stream.Null));
            _auth.IsAuthenticated.Returns(true);

            var model = BuildProviderViewModel();
            model.CurrentPath.Should().Be(Separator);
            model.UploadToCurrentPath.CanExecute(null).Should().BeTrue();
            model.UploadToCurrentPath.Execute(null);
            _provider.Received(1).Get(Separator);
        }

        [Fact]
        public void ShouldSetSelectedFileToNullWithCurrentPathChanges() 
        {
            var file = new FileModel { Name = "foo", Path = Separator + "foo", IsFolder = true };
            _provider.Get(Separator).Returns(Enumerable.Repeat(file, 1));
            _auth.IsAuthenticated.Returns(true);
            _provider.InitialPath.Returns(Separator);

            var model = BuildProviderViewModel();
            model.Refresh.Execute(null);

            model.Files.Should().NotBeEmpty();
            model.CurrentPath.Should().Be(Separator);

            model.SelectedFile = model.Files.First();
            model.SelectedFile.Should().NotBeNull();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);

            model.CurrentPath.Should().Be(Separator + "foo");
            model.SelectedFile.Should().BeNull();
            model.Open.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public void ShouldNotPublishNullCurrentPathValues()
        {
            var file = new FileModel { Name = "foo", Path = Separator + "foo", IsFolder = true };
            _provider.Get(Separator).Returns(Enumerable.Repeat(file, 1));
            _auth.IsAuthenticated.Returns(true);
            _provider.InitialPath.Returns(Separator);

            var model = BuildProviderViewModel();
            model.Refresh.Execute(null);
            
            model.Files.Should().NotBeEmpty();
            model.CurrentPath.Should().Be(Separator);
            model.Back.Execute(null);

            model.CurrentPath.Should().Be(Separator);
        }

        [Fact]
        public void ShouldSaveAndRestoreCurrentPath()
        {
            var initial = Separator + "foo";
            _state.CurrentPath = initial;

            var model = BuildProviderViewModel();
            model.CurrentPath.Should().Be(initial);
            model.Refresh.Execute(null);

            model.CurrentPath.Should().Be(initial);
            model.Back.Execute(null);

            model.CurrentPath.Should().Be(Separator);
            _state.CurrentPath.Should().Be(Separator);
        }
        
        private ProviderViewModel BuildProviderViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new ProviderViewModel(_state,
                x => _folder, x => _rename, 
                (x, y) => new FileViewModel(y, x),
                _auth, _files, _provider
            );
        }
    }
}