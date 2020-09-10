using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Uwp.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Models;
using ReactiveUI;
using Camelotia.Services;
using Camelotia.Presentation.AppState;

namespace Camelotia.Presentation.Uwp
{
    public sealed class Bootstrapper
    {
        public static IMainViewModel BuildMainViewModel()
        {
            return new MainViewModel(
                RxApp.SuspensionHost.GetAppState<MainState>(),
                new ProviderFactory(
                    new UniversalWindowsYandexAuthenticator(), 
                    Akavache.BlobCache.UserAccount, 
                    new[] { ProviderType.Yandex, ProviderType.VkDocs, ProviderType.Ftp, ProviderType.Sftp, ProviderType.GitHub }
                ),
                (state, provider) => new ProviderViewModel(state,
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
                     new UniversalWindowsFileManager(),
                    provider
                )
            );
        }
    }
}
