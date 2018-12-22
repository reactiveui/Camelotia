using System;
using System.Collections.Generic;
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
using Camelotia.Services.Models.Yandex;
using Newtonsoft.Json;

namespace Camelotia.Services.Providers
{
    public sealed class YandexFileSystemProvider : IProvider
    {
        private const string YandexAuthTokenUrl = "https://oauth.yandex.ru/token";
        private const string CloudApiDownloadFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/download?path=";
        private const string CloudApiUploadFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/upload?path=";
        private const string CloudApiGetPathBase = "https://cloud-api.yandex.net:443/v1/disk/resources?path=";
        private const string ClientSecret = "f14bfc0275a34ceea83d7de7f4b50898";
        private const string ClientId = "122661520b174cb5b85b4a3c26aa66f6";
        
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private readonly HttpClient _http = new HttpClient();
        private readonly IUriLauncher _uriLauncher;
        private readonly IListener _listener;

        public YandexFileSystemProvider(
            IUriLauncher uriLauncher,
            IListener listener)
        {
            _isAuthorized.OnNext(false);
            _uriLauncher = uriLauncher;
            _listener = listener;
        }

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
            var yaPath = from.Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = CloudApiDownloadFileUrl + encodedPath;
            using (var response = await _http.GetAsync(pathUrl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<YandexFileLoadResponse>(json);
                
                using (var file = await _http.GetAsync(content.Href).ConfigureAwait(false))
                using (var stream = await file.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    await stream.CopyToAsync(to).ConfigureAwait(false);
            }
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            var yaPath = Path.Combine(to, name).Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = CloudApiUploadFileUrl + encodedPath;
            using (var response = await _http.GetAsync(pathUrl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<YandexFileLoadResponse>(json);

                var httpContent = new StreamContent(from);
                using (var file = await _http.PutAsync(content.Href, httpContent).ConfigureAwait(false))
                    file.EnsureSuccessStatusCode();
            }
        }

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

        private async Task<string> GetAuthenticationCode()
        {
            var server = $"http://{IPAddress.Loopback}:{3000}/";
            _listener.Start(IPAddress.Loopback, 3000);
            
            var uriString = GetYandexAuthCodeUrl(server);
            var uri = new Uri(uriString);
            await _uriLauncher.LaunchUri(uri);

            var code = await _listener.GetCode();
            _listener.Stop();
            return code;
        }

        private static string GetYandexAuthCodeUrl(string redirect)
        {
            return "https://oauth.yandex.ru/authorize?response_type=code" +
                   $"&client_id={ClientId}&redirect_url={redirect}";
        }
    }
}