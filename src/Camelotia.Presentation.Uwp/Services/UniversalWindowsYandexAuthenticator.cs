using Camelotia.Services.Interfaces;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalWindowsYandexAuthenticator : IAuthenticator
    {
        private TaskCompletionSource<string> _taskCompletionSource;

        public GrantType GrantType => GrantType.AccessToken;

        public Task<string> ReceiveCode(Uri uri, Uri returnUri) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveToken(Uri uri)
        {
            _taskCompletionSource = new TaskCompletionSource<string>();
            await WebView.ClearTemporaryWebDataAsync();
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
            if (!currentUriString.Contains("#"))
                return;

            sender.NavigationCompleted -= OnNavigationCompleted;
            var token = currentUriString
                .Split('#')[1]
                .Split('&')[0]
                .Split('=')[1];

            _taskCompletionSource.SetResult(token);
            sender.Visibility = Visibility.Collapsed;
        }

        private static TControl FindControl<TControl>(UIElement parent) where TControl : FrameworkElement
        {
            var targetType = typeof(TControl);
            if (parent == null) return null;
            if (parent.GetType() == targetType)
                return (TControl)parent;

            var result = default(TControl);
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = (UIElement)VisualTreeHelper.GetChild(parent, i);
                var rec = FindControl<TControl>(child);
                if (rec == null) continue;
                result = rec;
                break;
            }
            return result;
        }
    }
}
