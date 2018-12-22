using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class AvaloniaYandexAuthenticator : IYandexAuthenticator
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";

        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Code;

        public Task<string> ReceiveYandexToken(Uri uri) => throw new PlatformNotSupportedException();

        public async Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port)
        {
            var server = $"http://{address}:{port}/";
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
