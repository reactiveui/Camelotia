using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Newtonsoft.Json;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet;

namespace Camelotia.Services.Providers
{
    public sealed class VkontakteFileSystemProvider : IProvider
    {
        private readonly ReplaySubject<bool> _isAuthorized;
        private VkApi _api;
        
        public VkontakteFileSystemProvider()
        {
            _api = new VkApi();
            _isAuthorized = new ReplaySubject<bool>();
            _isAuthorized.OnNext(false);
        }

        public string Size => "Unknown";

        public string Name => "Vkontakte Documents";

        public string Description => "Vkontakte documents provider";

        public string InitialPath { get; } = "Documents";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => true;

        public bool SupportsOAuth => false;

        public Task OAuth() => Task.CompletedTask;

        public async Task DirectAuth(string login, string password)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            if (password == null) throw new ArgumentNullException(nameof(password));

            await _api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = 5560698,
                Login = login,
                Password = password,
                Settings = Settings.Documents
            });
            _isAuthorized.OnNext(_api.IsAuthorized);
        }
        
        public Task Logout()
        {
            _api = new VkApi();
            _isAuthorized.OnNext(_api.IsAuthorized);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var documents = await _api.Docs.GetAsync();
            return documents.Select(document =>
            {
                var size = string.Empty;
                if (document.Size.HasValue)
                    size = ByteConverter.BytesToString(document.Size.Value);
                return new FileModel(document.Title, document.Uri, false, size);
            });
        }

        public async Task DownloadFile(string from, Stream to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            var isValidUriString = Uri.IsWellFormedUriString(from, UriKind.Absolute);
            if (!isValidUriString) throw new InvalidOperationException("Uri is invalid.");
            
            using (var http = new HttpClient())
            using (var response = await http.GetAsync(from).ConfigureAwait(false))
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                await stream.CopyToAsync(to).ConfigureAwait(false);
        }

        public async Task UploadFile(string to, Stream from, string name)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var server = await _api.Docs.GetUploadServerAsync().ConfigureAwait(false);
            var uri = new Uri(server.UploadUrl);
            
            var bytes = await StreamToArray(from).ConfigureAwait(false);
            var ext = Path.GetFileNameWithoutExtension(name);
            if (ext == null) throw new ArgumentNullException(nameof(name));
            
            using (var response = await PostSingleFileAsync(uri, bytes, ext.Trim('.'), name))
            using (var reader = new StreamReader(response, Encoding.UTF8))
            {
                var message = await reader.ReadToEndAsync().ConfigureAwait(false);
                var json = JsonConvert.DeserializeObject<DocUploadResponse>(message);
                if (!string.IsNullOrWhiteSpace(json.File)) return;

                var error = $"Unable to upload {name}{ext} \n{message}";
                throw new Exception(error);
            }                
        }

        private static async Task<byte[]> StreamToArray(Stream stream)
        {
            using (var memory = new MemoryStream())
            {
                await stream.CopyToAsync(memory);
                return memory.ToArray();
            }
        }
        
        private static async Task<Stream> PostSingleFileAsync(Uri uri, byte[] bytes, string type, string name)
        {
            using (var http = new HttpClient())
            using (var multipartFormDataContent = new MultipartFormDataContent())
            using (var byteArrayContent = new ByteArrayContent(bytes))
            {
                byteArrayContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    FileName = name, 
                    Name = type
                };
                
                multipartFormDataContent.Add(byteArrayContent);
                var response = await http.PostAsync(uri, multipartFormDataContent).ConfigureAwait(false);
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
        }

        private class DocUploadResponse
        {
            [JsonProperty("file")]
            public string File { get; set; }
        }
    }
}