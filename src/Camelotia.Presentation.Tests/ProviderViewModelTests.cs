using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderViewModelTests
    {
        private static readonly string Separator = Path.DirectorySeparatorChar.ToString();
        private readonly ICreateFolderViewModel _createFolder = Substitute.For<ICreateFolderViewModel>();
        private readonly IRenameFileViewModel _renameFile = Substitute.For<IRenameFileViewModel>();
        private readonly IAuthViewModel _authViewModel = Substitute.For<IAuthViewModel>();
        private readonly IFileManager _fileManager = Substitute.For<IFileManager>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly TestScheduler _scheduler = new TestScheduler();
        
        [Fact]
        public void ShouldDisplayLoadingReadyIndicatorsProperly() 
        {
            var model = BuildProviderViewModel();
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeFalse();
                
            model.Refresh.Execute(null);
            _scheduler.AdvanceBy(2);
                
            model.IsLoading.Should().BeTrue();
            model.IsReady.Should().BeFalse();
            _scheduler.AdvanceBy(2);
                
            model.IsLoading.Should().BeFalse();
            model.IsReady.Should().BeTrue();
        }

        [Fact]
        public void ShouldDisplayCurrentPathProperly() 
        {
            _provider.InitialPath.Returns(Separator);
            
            var model = BuildProviderViewModel();
            model.IsCurrentPathEmpty.Should().BeFalse();
            model.CurrentPath.Should().Be(Separator);
            _scheduler.AdvanceBy(2);
                
            model.IsCurrentPathEmpty.Should().BeFalse();
            model.Files.Should().BeEmpty();
            model.Refresh.Execute(null);
            _scheduler.AdvanceBy(4);
                
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
            model.Logout.CanExecute(null).Should().BeFalse();
            
            _scheduler.AdvanceBy(2);
            model.Logout.CanExecute(null).Should().BeTrue();
            model.Logout.Execute(null);
            _provider.Received(1).Logout();
            
            authorized.OnNext(false);
            _scheduler.AdvanceBy(2);
            model.Logout.CanExecute(null).Should().BeFalse();            
        }

        [Fact]
        public void ShouldBeAbleToOpenSelectedPath() 
        {
            var file = new FileModel { Name = "foo", Path = Separator + "foo", IsFolder = true };
            _provider.Get(Separator).Returns(Enumerable.Repeat(file, 1));
            _authViewModel.IsAuthenticated.Returns(true);
            _provider.InitialPath.Returns(Separator);

            var model = BuildProviderViewModel();
            using (model.Activator.Activate())
            {
                _scheduler.AdvanceBy(3);
                model.Files.Should().NotBeEmpty();
                model.CurrentPath.Should().Be(Separator);
                
                model.SelectedFile = model.Files.First();
                model.Open.CanExecute(null).Should().BeTrue();
                model.Open.Execute(null);
                
                _scheduler.AdvanceBy(3);
                model.CurrentPath.Should().Be(Separator + "foo");

                model.Back.CanExecute(null).Should().BeTrue();
                model.Back.Execute(null);
                
                _scheduler.AdvanceBy(3);
                model.CurrentPath.Should().Be(Separator);
            }
        }

        [Fact]
        public void ShouldRefreshContentOfCurrentPathWhenFileIsUploaded() 
        {
            _provider.InitialPath.Returns(Separator);            
            _fileManager.OpenRead().Returns(("example", Stream.Null));
            _authViewModel.IsAuthenticated.Returns(true);

            var model = BuildProviderViewModel();
            model.CurrentPath.Should().Be(Separator);
            model.UploadToCurrentPath.CanExecute(null).Should().BeTrue();
            model.UploadToCurrentPath.Execute(null);
            
            _scheduler.AdvanceBy(2);
            _provider.Received(1).Get(Separator);
        }

        [Fact]
        public void ShouldSetSelectedFileToNullWithCurrentPathChanges() 
        {
            var file = new FileModel { Name = "foo", Path = Separator + "foo", IsFolder = true };
            _provider.Get(Separator).Returns(Enumerable.Repeat(file, 1));
            _authViewModel.IsAuthenticated.Returns(true);
            _provider.InitialPath.Returns(Separator);

            var model = BuildProviderViewModel();
            model.Refresh.Execute(null);

            _scheduler.AdvanceBy(3);
            model.Files.Should().NotBeEmpty();
            model.CurrentPath.Should().Be(Separator);

            model.SelectedFile = model.Files.First();
            model.SelectedFile.Should().NotBeNull();
            model.Open.CanExecute(null).Should().BeTrue();
            model.Open.Execute(null);

            _scheduler.AdvanceBy(4);
            model.CurrentPath.Should().Be(Separator + "foo");
            model.SelectedFile.Should().BeNull();
            model.Open.CanExecute(null).Should().BeFalse();
        }

        private ProviderViewModel BuildProviderViewModel()
        {
            return new ProviderViewModel(
                x => _createFolder,
                x => _renameFile,
                (x, y) => new FileViewModel(y, x), 
                _authViewModel,
                _fileManager,
                _provider,
                _scheduler,
                _scheduler
            );
        }
    }
}