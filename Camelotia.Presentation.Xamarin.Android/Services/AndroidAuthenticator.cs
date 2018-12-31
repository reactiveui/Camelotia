using System;
using System.Net;
using System.Threading.Tasks;
using Android.Content;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Xamarin.Droid.Services
{
    public sealed class AndroidAuthenticator : IAuthenticator
    {
        private readonly MainActivity _activity;

        public AndroidAuthenticator(MainActivity activity) => _activity = activity;

        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Token;

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveYandexToken(Uri uri)
        {
            var completion = new TaskCompletionSource<string>();
            var startWebActivity = new Intent(_activity, typeof(WebActivity));
            startWebActivity.PutExtra("url", uri.ToString());

            _activity.StartActivityForResult(startWebActivity, 42);
            var subscription = _activity
                .AuthenticationCodeReceived
                .Subscribe(completion.SetResult);

            var token = await completion.Task;
            subscription.Dispose();
            return token;
        }
    }
}
