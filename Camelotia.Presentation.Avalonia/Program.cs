using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Avalonia;
using Camelotia.Services.Providers;

namespace Camelotia.Presentation.Avalonia
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            AppBuilder
                .Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .Start<MainView>(() =>
                    new MainViewModel(
                        (provider, files, auth) => new ProviderViewModel(auth, files, provider),
                        provider => new AuthViewModel(
                            new DirectAuthViewModel(provider),
                            new OAuthViewModel(provider),
                            provider
                        ), 
                        new ProviderStorage(),
                        new AvaloniaFileManager()
                    )
                );
        }
    }
}
