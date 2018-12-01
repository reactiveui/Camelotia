using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class ProviderViewModelTests
    {
        private readonly IAuthViewModel _authViewModel = Substitute.For<IAuthViewModel>();
        private readonly IFileManager _fileManager = Substitute.For<IFileManager>();
        private readonly IProvider _provider = Substitute.For<IProvider>();
        
        [Fact]
        public void ShouldDisplayLoadingReadyIndicatorsProperly()
        {
            _provider.IsAuthorized.Returns(Observable.Return(true));
            _provider.Get("/").Returns(x => Observable
                .Return(Enumerable.Empty<FileModel>())
                .ToTask());

            new TestScheduler().With(scheduler =>
            {
                var model = BuildProviderViewModel();
                model.IsLoading.Should().BeFalse();
                model.IsReady.Should().BeFalse();
                
                model.Refresh.Execute(null);
                scheduler.AdvanceBy(2);
                
                model.IsLoading.Should().BeTrue();
                model.IsReady.Should().BeFalse();
                scheduler.AdvanceBy(2);
                
                model.IsLoading.Should().BeFalse();
                model.IsReady.Should().BeTrue();
            });
        }

        [Fact]
        public void ShouldDisplayCurrentPathProperly()
        {
            _provider.IsAuthorized.Returns(Observable.Return(true));
            _provider.Get("/").Returns(x => Observable
                .Return(Enumerable.Empty<FileModel>())
                .ToTask());

            new TestScheduler().With(scheduler =>
            {
                var model = BuildProviderViewModel();
                model.IsCurrentPathEmpty.Should().BeFalse();
                model.CurrentPath.Should().Be("/");
                scheduler.AdvanceBy(2);
                
                model.IsCurrentPathEmpty.Should().BeFalse();
                model.Files.Should().BeEmpty();
                model.Refresh.Execute(null);
                scheduler.AdvanceBy(4);
                
                model.IsCurrentPathEmpty.Should().BeTrue();
                model.CurrentPath.Should().Be("/");
                model.Files.Should().BeEmpty();
            });
        }

        private IProviderViewModel BuildProviderViewModel() => new ProviderViewModel(
            _authViewModel, _fileManager, _provider
        );
    }
}