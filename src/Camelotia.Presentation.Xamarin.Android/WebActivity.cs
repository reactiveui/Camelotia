using Android.App;
using Android.Content;
using Android.OS;
using Android.Webkit;
using System;

namespace Camelotia.Presentation.Xamarin.Droid
{
    [Activity(Label = "Authorization")]
    public class WebActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WebActivity);

            var webView = (WebView)FindViewById(Resource.Id.web_activity_web_view);
            webView.RequestFocus(Android.Views.FocusSearchDirection.Down);
            webView.SetWebViewClient(new WebViewObserver(token =>
            {
                var data = new Intent();
                data.PutExtra("token", token);
                SetResult(Result.Ok, data);
                Finish();
            }));

            var url = Intent.GetStringExtra("url");
            webView.ClearHistory();
            webView.ClearFormData();
            webView.ClearCache(true);

            CookieManager.Instance.RemoveAllCookie();
            webView.LoadUrl(url);
        }

        public class WebViewObserver : WebViewClient
        {
            private readonly Action<string> _tokenReceived;

            public WebViewObserver(Action<string> tokenReceived) => _tokenReceived = tokenReceived;

            public override void OnPageFinished(WebView view, string url)
            {
                base.OnPageFinished(view, url);
                if (!url.Contains('#')) return;
                var token = url
                    .Split('#')[1]
                    .Split('&')[0]
                    .Split('=')[1];
                _tokenReceived(token);
            }
        }
    }
}