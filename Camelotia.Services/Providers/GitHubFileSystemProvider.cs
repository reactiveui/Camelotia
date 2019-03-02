using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Octokit;

namespace Camelotia.Services.Providers
{
    public sealed class GitHubFileSystemProvider : IProvider
    {
        private const string GithubApplicationId = "my-cool-app";
        private readonly GitHubClient _gitHub = new GitHubClient(new ProductHeaderValue(GithubApplicationId));
        private readonly ISubject<bool> _isAuthenticated = new ReplaySubject<bool>(1);
        private readonly HttpClient _httpClient = new HttpClient();
        private string _currentUserName;
        
        public GitHubFileSystemProvider(Guid id)
        {
            Id = id;
            _isAuthenticated.OnNext(false);
        }

        public Guid Id { get; }
        
        public string Size => "Unknown";

        public string Name => "GitHub";

        public string Description => "GitHub repositories provider.";

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
                
                var repositories = await _gitHub.Repository
                    .GetAllForCurrent(request)
                    .ConfigureAwait(false);
                
                var repos =
                    from repo in repositories
                    let size = ByteConverter.BytesToString(repo.Size)
                    select new FileModel(repo.Name, repo.Name, true, size, repo.CreatedAt.UtcDateTime);
                
                return repos;
            }

            var details = GetRepositoryNameAndFilePath(path);
            var contents = await _gitHub.Repository.Content
                .GetAllContents(_currentUserName, details.Repository, details.Path)
                .ConfigureAwait(false);

            var files =
                from file in contents
                let size = ByteConverter.BytesToString(file.Size)
                let filePath = Path.Combine(details.Repository, file.Path)
                select new FileModel(file.Name, filePath, file.Type == "dir", size);

            return files;
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

        public Task CreateFolder(string path, string name)
        {
            throw new NotImplementedException();
        }

        public Task RenameFile(FileModel file, string name)
        {
            throw new NotImplementedException();
        }

        public Task UploadFile(string to, Stream from, string name) => throw new NotImplementedException();

        public Task Delete(FileModel file) => throw new NotImplementedException();

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
        
        private static string GetSha1Hash(Stream stream)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var hash = sha1.ComputeHash(stream);
                var hashStr = Convert.ToBase64String(hash);
                return hashStr.TrimEnd('=');
            }
        }
    }
}