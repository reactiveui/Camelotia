using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Services.Providers
{
    public sealed class YandexFileSystemProvider : IProvider
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";
        private const string ClientId = "122661520b174cb5b85b4a3c26aa66f6";
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private string _code;
        
        public string Size => "Unknown";

        public string Name => "Yandex Disk";

        public string Description => "Yandex Disk file provider";
        
        public Task<IEnumerable<FileModel>> Get(string path) => Task.FromResult(Enumerable.Empty<FileModel>());

        public Task DownloadFile(string from, Stream to) => Task.CompletedTask;

        public Task UploadFile(string to, Stream from, string name) => Task.CompletedTask;

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;

        public bool SupportsOAuth => true;
        
        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task Logout()
        {
            _code = null;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public async Task OAuth()
        {
            var listener = new HttpListener();
            var server = $"http://{IPAddress.Loopback}:{3000}/";
            listener.Prefixes.Add(server);
            listener.Start();
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = GetYandexAuthUrl(server)
                }
            };
            
            process.Start();
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            _code = code;
            
            var buffer = Encoding.UTF8.GetBytes(SuccessContent);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

            _isAuthorized.OnNext(true);
            context.Response.Close();
            listener.Close();
        }

        private static string GetYandexAuthUrl(string redirect) => 
             "https://oauth.yandex.ru/authorize?response_type=code" + 
            $"&client_id={ClientId}&redirect_url={redirect}";
    }
}