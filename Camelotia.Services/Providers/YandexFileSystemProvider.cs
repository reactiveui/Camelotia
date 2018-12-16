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
        private const string CloudApiDownloadFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/download?path=";
        private const string CloudApiGetPathBase = "https://cloud-api.yandex.net:443/v1/disk/resources?path=";
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";
        private const string ClientSecret = "f14bfc0275a34ceea83d7de7f4b50898";
        private const string ClientId = "122661520b174cb5b85b4a3c26aa66f6";
        
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private readonly HttpClient _http = new HttpClient();
        
        public string Size => "Unknown";

        public string Name => "Yandex Disk";
        
        public string Description => "Yandex Disk file provider";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;

        public bool SupportsOAuth => true;

        public string InitialPath => Path.DirectorySeparatorChar.ToString();

        public Task DirectAuth(string login, string password) => Task.CompletedTask;
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));

            var yaPath = path.Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = CloudApiGetPathBase + encodedPath;
            using (var response = await _http.GetAsync(pathUrl).ConfigureAwait(false))
            {
                var json = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                var content = JsonConvert.DeserializeObject<YandexContentResponse>(json);
                var models = content.Embedded.Items
                    .Select(file => new FileModel(
                        file.Name,
                        file.Path.Replace("disk:", ""),
                        file.Type == "dir",
                        false,
                        ByteConverter.BytesToString(file.Size)));

                return models;
            }
        }

        public async Task DownloadFile(string from, Stream to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            var encodedPath = WebUtility.UrlEncode(from);
            var pathUrl = CloudApiDownloadFileUrl + encodedPath;
            using (var response = await _http.GetAsync(pathUrl).ConfigureAwait(false))
            {
                var json = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                
                var content = JsonConvert.DeserializeObject<YandexFileDownloadResponse>(json);
                using (var file = await _http.GetAsync(content.Href).ConfigureAwait(false))
                using (var stream = await file.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    await stream.CopyToAsync(to).ConfigureAwait(false);
            }
        }

        public Task UploadFile(string to, Stream from, string name) => Task.CompletedTask;

        public Task Logout()
        {
            _http.DefaultRequestHeaders.Clear();
            _isAuthorized.OnNext(false);
            return Task.CompletedTask;
        }

        public async Task OAuth()
        {
            var code = await GetAuthenticationCode();
            var token = await GetAuthenticationToken(code);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);
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

        private class YandexTokenAuthResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }

        private class YandexContentResponse
        {
            [JsonProperty("_embedded")]
            public YandexContentItemsResponse Embedded { get; set; }
        }

        private class YandexContentItemsResponse
        {
            [JsonProperty("items")]
            public IList<YandexContentItemResponse> Items { get; set; }
        }

        private class YandexContentItemResponse
        {
            [JsonProperty("path")]
            public string Path { get; set; }
            
            [JsonProperty("type")]
            public string Type { get; set; }
            
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("size")]
            public int Size { get; set; }
            
            [JsonProperty("created")]
            public DateTime Created { get; set; }
        }

        private class YandexFileDownloadResponse
        {
            [JsonProperty("href")]
            public string Href { get; set; }
        }
    }
}