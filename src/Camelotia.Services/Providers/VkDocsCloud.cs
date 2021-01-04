using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Newtonsoft.Json;
using VkNet;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace Camelotia.Services.Providers
{
    public sealed class VkDocsCloud : ICloud, IDisposable
    {
        private readonly ReplaySubject<bool> _isAuthorized = new ReplaySubject<bool>();
        private readonly VkDocsCloudOptions _options;
        private IVkApi _api = new VkApi();

        public VkDocsCloud(CloudParameters model, VkDocsCloudOptions options)
        {
            Parameters = model;
            _options = options;
            _isAuthorized.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public CloudParameters Parameters { get; }

        public long? Size => null;

        public Guid Id => Parameters.Id;

        public string Name => Parameters.Type.ToString();

        public DateTime Created => Parameters.Created;

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => true;

        public bool SupportsHostAuth => false;

        public bool SupportsOAuth => false;

        public bool CanCreateFolder => false;

        public string InitialPath => Path.DirectorySeparatorChar.ToString();

        public Task OAuth() => Task.CompletedTask;

        public Task HostAuth(string address, int port, string login, string password) => Task.CompletedTask;

        public async Task DirectAuth(string login, string password)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            if (password == null) throw new ArgumentNullException(nameof(password));

            await _api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = _options.ApplicationId,
                Login = login,
                Password = password,
                Settings = Settings.Documents
            }).ConfigureAwait(false);

            Parameters.Token = _api.Token;
            _isAuthorized.OnNext(_api.IsAuthorized);
        }

        public Task Logout()
        {
            _api = new VkApi();
            Parameters.Token = null;
            _isAuthorized.OnNext(_api.IsAuthorized);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<FileModel>> GetFiles(string path)
        {
            var documents = await _api.Docs.GetAsync().ConfigureAwait(false);
            return documents.Select(document => new FileModel
            {
                Name = document.Title,
                Path = document.Id.ToString(),
                IsFolder = false,
                Size = document.Size ?? 0,
                Modified = document.Date
            });
        }

        public Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path) => throw new NotImplementedException();

        public async Task DownloadFile(string from, Stream to)
        {
            var id = long.Parse(from);
            var users = await _api.Users.GetAsync(Array.Empty<long>()).ConfigureAwait(false);
            var currentUser = users.First();

            var documents = await _api.Docs.GetByIdAsync(new[] { new Document { Id = id, OwnerId = currentUser.Id } }).ConfigureAwait(false);
            var document = documents.First();

            var uri = document.Uri;
            var isValidUriString = Uri.IsWellFormedUriString(uri, UriKind.Absolute);
            if (!isValidUriString) throw new InvalidOperationException("Uri is invalid.");

            using (var http = new HttpClient())
            using (var response = await http.GetAsync(uri).ConfigureAwait(false))
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                await stream.CopyToAsync(to).ConfigureAwait(false);

            await to.FlushAsync().ConfigureAwait(false);
            to.Close();
        }

        public Task CreateFolder(string path, string name) => throw new NotSupportedException("Folders not supported.");

        public Task RenameFile(string path, string name) => throw new NotSupportedException("Rename not supported.");

        public async Task UploadFile(string to, Stream from, string name)
        {
            var server = await _api.Docs.GetUploadServerAsync().ConfigureAwait(false);
            var uri = new Uri(server.UploadUrl);

            var bytes = await StreamToArray(from).ConfigureAwait(false);
            var ext = Path.GetFileNameWithoutExtension(name);
            if (ext == null) throw new ArgumentNullException(nameof(name));

            using var response = await PostSingleFileAsync(uri, bytes, ext.Trim('.'), name).ConfigureAwait(false);
            using var reader = new StreamReader(response, Encoding.UTF8);
            var message = await reader.ReadToEndAsync().ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<DocUploadResponse>(message);
            if (!string.IsNullOrWhiteSpace(json.File)) return;

            var error = $"Unable to upload {name}{ext} \n{message}";
            throw new Exception(error);
        }

        public async Task Delete(string path, bool isFolder)
        {
            var id = long.Parse(path);
            var users = await _api.Users.GetAsync(Array.Empty<long>()).ConfigureAwait(false);
            var currentUser = users[0];
            await _api.Docs.DeleteAsync(currentUser.Id, id).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _isAuthorized.Dispose();
            _api.Dispose();
        }

        private static async Task<byte[]> StreamToArray(Stream stream)
        {
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory).ConfigureAwait(false);
            return memory.ToArray();
        }

        private static async Task<Stream> PostSingleFileAsync(Uri uri, byte[] bytes, string type, string name)
        {
            using var http = new HttpClient();
            using var multipartFormDataContent = new MultipartFormDataContent();
            using var byteArrayContent = new ByteArrayContent(bytes);
            byteArrayContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = name,
                Name = type
            };

            multipartFormDataContent.Add(byteArrayContent);
            var response = await http.PostAsync(uri, multipartFormDataContent).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private async void EnsureLoggedInIfTokenSaved()
        {
            var token = Parameters.Token;
            if (string.IsNullOrWhiteSpace(token) || _api.IsAuthorized) return;
            await _api.AuthorizeAsync(new ApiAuthParams { AccessToken = token }).ConfigureAwait(false);
            _isAuthorized.OnNext(true);
        }

        private class DocUploadResponse
        {
            [JsonProperty("file")]
            public string File { get; set; }
        }
    }
}
