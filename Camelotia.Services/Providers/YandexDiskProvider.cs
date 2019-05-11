using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Models.Yandex;
using Newtonsoft.Json;

namespace Camelotia.Services.Providers
{
    public sealed class YandexDiskProvider : IProvider
    {
        private const string YandexAuthTokenUrl = "https://oauth.yandex.ru/token";
        private const string ApiMoveFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/move";
        private const string ApiDownloadFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/download?path=";
        private const string ApiUploadFileUrl = "https://cloud-api.yandex.net/v1/disk/resources/upload?path=";
        private const string ApiGetPathBase = "https://cloud-api.yandex.net:443/v1/disk/resources?path=";
        private const string HashAuthClientId = "7762e3fccbe3431db2652a8434618790";
        private const string CodeAuthClientSecret = "317a14f5491447e8bd3a9e7e14ce46cd";
        private const string CodeAuthClientId = "122661520b174cb5b85b4a3c26aa66f6";
        
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>(1);
        private readonly HttpClient _http = new HttpClient();
        private readonly IAuthenticator _authenticator;
        private readonly IBlobCache _blobCache;

        public YandexDiskProvider(Guid id, IAuthenticator authenticator, IBlobCache blobCache)
        {
            Id = id;
            _blobCache = blobCache;
            _authenticator = authenticator;
            _isAuthorized.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public Guid Id { get; }
        
        public string Size => "Unknown";

        public string Name => "Yandex Disk";
        
        public string Description => "Yandex Disk file provider.";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => false;
        
        public bool SupportsHostAuth => false;

        public bool SupportsOAuth => true;
        
        public bool CanCreateFolder => true;

        public string InitialPath => Path.DirectorySeparatorChar.ToString();

        public Task HostAuth(string address, int port, string login, string password) => Task.CompletedTask;

        public Task DirectAuth(string login, string password) => Task.CompletedTask;
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            var yaPath = path.Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = ApiGetPathBase + encodedPath;
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
                        ByteConverter.BytesToString(file.Size),
                        file.Created));

                return models;
            }
        }

        public async Task DownloadFile(string from, Stream to)
        {
            var yaPath = from.Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = ApiDownloadFileUrl + encodedPath;
            using (var response = await _http.GetAsync(pathUrl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var content = JsonConvert.DeserializeObject<YandexFileLoadResponse>(json);
                
                using (var file = await _http.GetAsync(content.Href).ConfigureAwait(false))
                using (var stream = await file.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    await stream.CopyToAsync(to).ConfigureAwait(false);

                await to.FlushAsync();
                to.Close();
            }
        }

        public async Task CreateFolder(string path, string name)
        {
            var directory = Path.Combine(path, name).Replace("\\", "/");
            var encoded = WebUtility.UrlEncode(directory);
            var pathUrl = ApiGetPathBase + encoded;
            using (var response = await _http.PutAsync(pathUrl, null).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public async Task RenameFile(FileModel file, string name)
        {
            var directoryName = Path.GetDirectoryName(file.Path);
            var fromPath = WebUtility.UrlEncode(file.Path);
            var toPath = Path.Combine(directoryName, name);
            
            var pathUrl = $"{ApiMoveFileUrl}?from={fromPath}&path={toPath}";
            using (var response = await _http.PostAsync(pathUrl, null).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            var yaPath = Path.Combine(to, name).Replace("\\", "/");
            var encodedPath = WebUtility.UrlEncode(yaPath);
            var pathUrl = ApiUploadFileUrl + encodedPath;
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

        public async Task Delete(FileModel file)
        {
            var encodedPath = WebUtility.UrlEncode(file.Path);
            var pathUrl = ApiGetPathBase + encodedPath;
            using (var response = await _http.DeleteAsync(pathUrl).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public async Task Logout()
        {
            var persistentId = Id.ToString();
            var model = await _blobCache.GetObject<ProviderModel>(persistentId);
            model.Token = null;
            
            await _blobCache.InsertObject(persistentId, model);
            _http.DefaultRequestHeaders.Clear();
            _isAuthorized.OnNext(false);
        }

        public async Task OAuth()
        {
            var persistentId = Id.ToString();
            var token = await GetAuthenticationToken();
            var model = await _blobCache.GetObject<ProviderModel>(persistentId);
            model.Token = token;
            
            await _blobCache.InsertObject(persistentId, model);
            ApplyTokenToHeaders(token);
            _isAuthorized.OnNext(true);
        }
        
        private async void EnsureLoggedInIfTokenSaved()
        {
            var persistentId = Id.ToString();
            var model = await _blobCache.GetOrFetchObject(persistentId, () => Task.FromResult(default(ProviderModel)));
            var token = model?.Token;
            
            if (string.IsNullOrWhiteSpace(token)) return;
            ApplyTokenToHeaders(token);
            _isAuthorized.OnNext(true);
        }

        private void ApplyTokenToHeaders(string token)
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);
        }

        private async Task<string> GetAuthenticationToken()
        {
            switch (_authenticator.GrantType)
            {
                case GrantType.AuthorizationCode:
                    var server = $"http://{IPAddress.Loopback}:{3000}/";
                    var codeUri = GetYandexAuthCodeUrl(server);
                    var code = await _authenticator.ReceiveCode(codeUri, new Uri(server));
                    return await GetAuthenticationTokenFromCode(code);
                case GrantType.AccessToken:
                    var tokenUri = GetYandexAuthTokenUrl();
                    return await _authenticator.ReceiveToken(tokenUri);
                default: throw new InvalidOperationException();
            }
        }

        private static async Task<string> GetAuthenticationTokenFromCode(string code)
        {
            using (var http = new HttpClient())
            using (var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = CodeAuthClientId,
                ["client_secret"] = CodeAuthClientSecret,
                ["grant_type"] = "authorization_code"
            }))
            using (var response = await http.PostAsync(YandexAuthTokenUrl, content).ConfigureAwait(false))
            {
                var token = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<YandexTokenAuthResponse>(token);
                response.EnsureSuccessStatusCode();
                return model.AccessToken;
            }
        }

        private static Uri GetYandexAuthTokenUrl()
        {
            var uri = "https://oauth.yandex.ru/authorize?response_type=token" +
                     $"&client_id={HashAuthClientId}";
            return new Uri(uri);
        }

        private static Uri GetYandexAuthCodeUrl(string redirect)
        {
            var uri = "https://oauth.yandex.ru/authorize?response_type=code" +
                     $"&client_id={CodeAuthClientId}&redirect_url={redirect}";
            return new Uri(uri);
        }
    }
}