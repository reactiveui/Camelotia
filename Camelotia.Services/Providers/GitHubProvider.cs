using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Akavache;
using Octokit;

namespace Camelotia.Services.Providers
{
    public sealed class GitHubProvider : IProvider
    {
        private const string GithubApplicationId = "my-cool-app";
        private readonly GitHubClient _gitHub = new GitHubClient(new ProductHeaderValue(GithubApplicationId));
        private readonly ISubject<bool> _isAuthenticated = new ReplaySubject<bool>(1);
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IBlobCache _blobCache;
        private readonly ProviderModel _model;
        private string _currentUserName;
        
        public GitHubProvider(ProviderModel model, IBlobCache blobCache)
        {
            _model = model;
            _blobCache = blobCache;
            _isAuthenticated.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public long? Size => null;

        public Guid Id => _model.Id;

        public string Name => _model.Type;

        public DateTime Created => _model.Created;

        public string InitialPath => string.Empty;

        public IObservable<bool> IsAuthorized => _isAuthenticated;
        
        public bool SupportsDirectAuth => true;

        public bool SupportsHostAuth => false;

        public bool SupportsOAuth => false;
        
        public bool CanCreateFolder => false;

        public Task HostAuth(string address, int port, string login, string password) => Task.CompletedTask;

        public Task OAuth() => Task.CompletedTask;

        public async Task DirectAuth(string login, string password)
        {
            _currentUserName = login;
            _gitHub.Credentials = new Credentials(login, password);
            await _gitHub.User.Current();
            
            var persistentId = Id.ToString();
            var model = await _blobCache.GetObject<ProviderModel>(persistentId);
            model.Token = password;
            model.User = login;
            
            await _blobCache.InsertObject(persistentId, model);
            _isAuthenticated.OnNext(true);
        }

        public Task Logout()
        {
            _currentUserName = null;
            _gitHub.Credentials = Credentials.Anonymous;
            _isAuthenticated.OnNext(false);
            return Task.CompletedTask;
        }
        
        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                var request = new RepositoryRequest
                {
                    Type = RepositoryType.Owner,
                    Sort = RepositorySort.Updated
                };
                var repositories = await _gitHub.Repository.GetAllForCurrent(request).ConfigureAwait(false);
                return repositories.Select(repo => new FileModel
                {
                    IsFolder = true,
                    Modified = repo.CreatedAt.UtcDateTime,
                    Name = repo.Name,
                    Path = repo.Name,
                    Size = repo.Size
                });
            }

            var details = GetRepositoryNameAndFilePath(path);
            var contents = await _gitHub.Repository.Content
                .GetAllContents(_currentUserName, details.Repository, details.Path)
                .ConfigureAwait(false);

            return contents.Select(file => new FileModel
            {
                Name = file.Name,
                IsFolder = file.Type == "dir",
                Path = Path.Combine(details.Repository, file.Path),
                Size = file.Size
            });
        }

        public async Task DownloadFile(string from, Stream to)
        {
            var details = GetRepositoryNameAndFilePath(from);
            var contents = await _gitHub.Repository.Content
                .GetAllContents(_currentUserName, details.Repository, details.Path)
                .ConfigureAwait(false);

            var downloadUrl = contents.First().DownloadUrl;
            using (var file = await _httpClient.GetAsync(downloadUrl).ConfigureAwait(false))
            using (var stream = await file.Content.ReadAsStreamAsync().ConfigureAwait(false))
                await stream.CopyToAsync(to).ConfigureAwait(false);

            await to.FlushAsync();
            to.Close();
        }

        public Task CreateFolder(string path, string name) => throw new NotImplementedException();

        public Task RenameFile(string path, string name) => throw new NotImplementedException();

        public Task UploadFile(string to, Stream from, string name) => throw new NotImplementedException();

        public Task Delete(string path, bool isFolder) => throw new NotImplementedException();

        private async void EnsureLoggedInIfTokenSaved()
        {
            var persistentId = Id.ToString();
            var model = await _blobCache.GetOrFetchObject(persistentId, () => Task.FromResult(default(ProviderModel)));
            if (model?.User == null || model?.Token == null) return;   
            _gitHub.Credentials = new Credentials(model.User, model.Token);
            _currentUserName = model.User;
            _isAuthenticated.OnNext(true);
        }

        private static (string Repository, string Path, string Separator) GetRepositoryNameAndFilePath(string input)
        {
            var separator = Path.DirectorySeparatorChar;
            var parts = input.Split(separator);
            var repositoryName = parts.First();
            var pathParts = Enumerable
                .Repeat(separator.ToString(), 1)
                .Concat(parts.Skip(1))
                .ToArray();

            var path = Path.Combine(pathParts);
            return (repositoryName, path, separator.ToString());
        }
    }
}