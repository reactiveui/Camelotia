using System;
using System.Threading.Tasks;
using Android.Content;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Xamarin.Droid.Services
{
    public sealed class AndroidYandexAuthenticator : IAuthenticator
    {
        private readonly MainActivity _activity;

        public AndroidYandexAuthenticator(MainActivity activity) => _activity = activity;

        public GrantType GrantType => GrantType.AccessToken;

        public Task<string> ReceiveCode(Uri uri, Uri returnUri) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveToken(Uri uri)
        {
            var completion = new TaskCompletionSource<string>();
            var startWebActivity = new Intent(_activity, typeof(WebActivity));
            startWebActivity.PutExtra("url", uri.ToString());

            _activity.StartActivityForResult(startWebActivity, 42);
            var subscription = _activity
                .AuthenticationCodeReceived
                .Subscribe(completion.SetResult);

            var token = await completion.Task.ConfigureAwait(false);
            subscription.Dispose();
            return token;
        }
    }
}
