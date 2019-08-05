using Camelotia.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Camelotia.Presentation.Wpf.Services
{
    public sealed class WindowsPresentationYandexAuthenticator : IAuthenticator
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";

        public GrantType GrantType => GrantType.AuthorizationCode;

        public Task<string> ReceiveToken(Uri uri) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveCode(Uri uri, Uri returnUri)
        {
            var server = returnUri.ToString();
            var listener = new HttpListener();
            listener.Prefixes.Add(server);
            listener.Start();
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = uri.ToString()
                }
            }
            .Start();

            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];

            var buffer = Encoding.UTF8.GetBytes(SuccessContent);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

            context.Response.Close();
            listener.Close();
            return code;
        }
    }
}
