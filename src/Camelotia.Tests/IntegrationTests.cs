using System.Reactive.Concurrency;
using Akavache;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests
{
    public sealed class IntegrationTests
    {
        private readonly IAuthenticator _authenticator = Substitute.For<IAuthenticator>();
        private readonly IBlobCache _cache = Substitute.For<IBlobCache>();
        private readonly IFileManager _files = Substitute.For<IFileManager>();
        private readonly MainState _state = new MainState();

        [Fact]
        public void ShouldWireUpAppViewModels()
        {
            _state.Clouds.AddOrUpdate(new CloudState
            {
                Type = CloudType.Local,
                CreateFolderState = new CreateFolderState
                {
                    Name = "Example",
                    IsVisible = true
                }
            });
            
            var main = BuildMainViewModel();
            main.SupportedTypes.Should().Contain(CloudType.Local);
            main.SelectedSupportedType.Should().Be(CloudType.Local);
            main.Clouds.Should().NotBeEmpty();
            main.Clouds.Count.Should().Be(1);
            
            var provider = main.Clouds[0];
            provider.Name.Should().Be("Local");
            provider.CanInteract.Should().BeFalse();
            provider.Rename.IsVisible.Should().BeFalse();
            provider.Folder.IsVisible.Should().BeTrue();
            provider.Folder.Name.Should().Be("Example");
        }
        
        private IMainViewModel BuildMainViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new MainViewModel(
                _state,
                new CloudFactory(_authenticator, _cache), 
                (state, provider) => new CloudViewModel(state,
                    owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
                    owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
                    (file, owner) => new FileViewModel(owner, file),
                    (folder, owner) => new FolderViewModel(owner, folder),
                    new AuthViewModel(
                        new DirectAuthViewModel(state.AuthState.DirectAuthState, provider),
                        new HostAuthViewModel(state.AuthState.HostAuthState, provider),
                        new OAuthViewModel(provider),
                        provider
                    ), 
                    _files, provider
                )
            );
        }
    }
}