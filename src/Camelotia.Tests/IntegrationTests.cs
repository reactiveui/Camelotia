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
            _state.Providers.AddOrUpdate(new ProviderState
            {
                Type = ProviderType.Local,
                CreateFolderState = new CreateFolderState
                {
                    Name = "Example"
                }
            });
            
            var main = ComposeMainViewModel();
            main.SupportedTypes.Should().Contain(ProviderType.Local);
            main.SelectedSupportedType.Should().Be(ProviderType.Local);
            main.Providers.Should().NotBeEmpty();
            main.Providers.Count.Should().Be(1);
            
            var provider = main.Providers[0];
            provider.Name.Should().Be("Local");
            provider.CanInteract.Should().BeTrue();
            provider.Rename.IsVisible.Should().BeFalse();
            provider.Folder.IsVisible.Should().BeFalse();
            provider.Folder.Name.Should().Be("Example");
        }
        
        private IMainViewModel ComposeMainViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new MainViewModel(
                _state,
                new ProviderFactory(_authenticator, _cache), 
                (state, provider) => new ProviderViewModel(state,
                    owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
                    owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
                    (file, owner) => new FileViewModel(owner, file),
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