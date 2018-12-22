using Camelotia.Services.Interfaces;
using System.Threading.Tasks;
using System.Net;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalWindowsAuthenticator : IAuthenticator
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";
        private TaskCompletionSource<string> _taskCompletionSource;

        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Token;

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveYandexToken(Uri uri)
        {
            _taskCompletionSource = new TaskCompletionSource<string>();
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            {
                var root = Window.Current.Content;
                var web = FindControl<WebView>(root);
                if (web == null) throw new Exception("WebView named AuthenticationWebView wasn't found.");

                web.Visibility = Visibility.Visible;
                web.NavigationCompleted += OnNavigationCompleted;
                web.Navigate(uri);
            });

            return await _taskCompletionSource.Task;
        }

        private void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var currentUriString = args.Uri.ToString();
            if (!currentUriString.Contains('#'))
                return;

            sender.NavigationCompleted -= OnNavigationCompleted;
            var token = currentUriString
                .Split('#')[1]
                .Split('&')[0]
                .Split('=')[1];

            _taskCompletionSource.SetResult(token);
            sender.Visibility = Visibility.Collapsed;
        }

        public static TControl FindControl<TControl>(UIElement parent) where TControl : FrameworkElement
        {
            var targetType = typeof(TControl);
            if (parent == null) return null;
            if (parent.GetType() == targetType)
                return (TControl)parent;

            var result = default(TControl);
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = (UIElement)VisualTreeHelper.GetChild(parent, i);
                var rec = FindControl<TControl>(child);
                if (rec != null)
                {
                    result = rec;
                    break;
                }
            }
            return result;
        }
    }
}
