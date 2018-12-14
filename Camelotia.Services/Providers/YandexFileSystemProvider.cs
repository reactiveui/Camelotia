using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Newtonsoft.Json;

namespace Camelotia.Services.Providers
{
    public sealed class YandexFileSystemProvider : IProvider
    {
        private const string YandexAuthTokenUrl = "https://oauth.yandex.ru/token";
        private const string CloudApiGetPathBase = "https://cloud-api.yandex.net:443/v1/disk/resources?path=";
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";
        private const string ClientSecret = "f14bfc0275a34ceea83d7de7f4b50898";
        private const string ClientId = "122661520b174cb5b85b4a3c26aa66f6";
        
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private string _accessToken;
        
        public string Size => "Unknown";

        public string Name => "Yandex Disk";
        
        public string Description => "Yandex Disk file provider";
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _accessToken);

                var encodedPath = WebUtility.UrlEncode(path);
                var pathUrl = CloudApiGetPathBase + encodedPath;
                using (var response = await http.GetAsync(pathUrl).ConfigureAwait(false))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    var content = JsonConvert.DeserializeObject<YandexContentResponse>(json);
                    var models = content.Embedded.Items
                        .Select(file => new FileModel(
                            file.Name,
                            file.Path.Replace("disk:", ""),
                            file.Type == "dir",
                            file.Size));

                    return models;
                }
            }
        }

        public Task DownloadFile(string from, Stream to) => Task.CompletedTask;

        public Task UploadFile(string to, Stream from, string name) => Task.CompletedTask;

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;

        public bool SupportsOAuth => true;
        
        public Task DirectAuth(string login, string password) => Task.CompletedTask;

        public Task Logout()
        {
            _accessToken = string.Empty;
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public async Task OAuth()
        {
            var code = await GetAuthenticationCode();
            var token = await GetAuthenticationToken(code);
            _accessToken = token;
            _isAuthorized.OnNext(true);
        }

        private static async Task<string> GetAuthenticationToken(string code)
        {
            var form = new Dictionary<string, string>
            {
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret
            };
            
            using (var http = new HttpClient())
            using (var content = new FormUrlEncodedContent(form))
            using (var response = await http.PostAsync(YandexAuthTokenUrl, content).ConfigureAwait(false))
            {
                var token = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<YandexTokenAuthResponse>(token);
                response.EnsureSuccessStatusCode();
                return model.AccessToken;
            }
        }

        private static async Task<string> GetAuthenticationCode()
        {
            var listener = new HttpListener();
            var server = $"http://{IPAddress.Loopback}:{3000}/";
            listener.Prefixes.Add(server);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = GetYandexAuthCodeUrl(server)
                }
            };
            
            listener.Start();
            process.Start();
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            
            var buffer = Encoding.UTF8.GetBytes(SuccessContent);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
            listener.Close();
            return code;
        }

        private static string GetYandexAuthCodeUrl(string redirect)
        {
            return "https://oauth.yandex.ru/authorize?response_type=code" +
                   $"&client_id={ClientId}&redirect_url={redirect}";
        }

        internal class YandexTokenAuthResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }

        internal class YandexContentResponse
        {
            [JsonProperty("_embedded")]
            public YandexContentItemsResponse Embedded { get; set; }
        }

        internal class YandexContentItemsResponse
        {
            [JsonProperty("items")]
            public IList<YandexContentItemResponse> Items { get; set; }
        }

        internal class YandexContentItemResponse
        {
            [JsonProperty("path")]
            public string Path { get; set; }
            
            [JsonProperty("type")]
            public string Type { get; set; }
            
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("size")]
            public string Size { get; set; }
            
            [JsonProperty("created")]
            public DateTime Created { get; set; }
        }
    }
}