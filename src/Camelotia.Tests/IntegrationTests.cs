using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests
{
    public sealed class IntegrationTests
    {
        private readonly IObjectBlobCache _blobCache = Substitute.For<IObjectBlobCache>();
        private readonly IFileManager _files = Substitute.For<IFileManager>();

        [Fact]
        public void ShouldWireUpAppViewModels()
        {
            var main = ComposeMainViewModel();
            using (main.Activator.Activate())
            {
                main.SupportedTypes.Should().Contain(nameof(LocalProvider));
                main.SelectedSupportedType.Should().Be(nameof(LocalProvider));
                main.Providers.Should().BeEmpty();
                main.Add.Execute(null);

                main.Providers.Should().NotBeEmpty();
                main.Providers[0].Name.Should().Be(nameof(LocalProvider));
                main.Providers[0].CanInteract.Should().BeTrue();
                main.Providers[0].Rename.IsVisible.Should().BeFalse();
                main.Providers[0].Folder.IsVisible.Should().BeFalse();
            }
        }
        
        private MainViewModel ComposeMainViewModel()
        {
            RxApp.MainThreadScheduler = Scheduler.Immediate;
            RxApp.TaskpoolScheduler = Scheduler.Immediate;
            return new MainViewModel(
                (provider, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, provider),
                    model => new RenameFileViewModel(model, provider),
                    (file, model) => new FileViewModel(model, file),
                    auth, _files, provider
                ),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(provider),
                    new HostAuthViewModel(provider),
                    new OAuthViewModel(provider),
                    provider
                ),
                new AkavacheStorage(
                    new Dictionary<string, Func<ProviderModel, IProvider>>
                    {
                        [nameof(LocalProvider)] = id => new LocalProvider(id)
                    },
                    _blobCache)
            );
        }
    }
}