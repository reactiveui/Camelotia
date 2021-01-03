using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Octokit;

namespace Camelotia.Services.Providers
{
    public sealed class GitHubCloud : ICloud, IDisposable
    {
        private const string GithubApplicationId = "my-cool-app";
        private readonly GitHubClient _gitHub = new GitHubClient(new ProductHeaderValue(GithubApplicationId));
        private readonly ISubject<bool> _isAuthenticated = new ReplaySubject<bool>(1);
        private readonly HttpClient _httpClient = new HttpClient();
        private string _currentUserName;

        public GitHubCloud(CloudParameters model)
        {
            Parameters = model;
            _isAuthenticated.OnNext(false);
            EnsureLoggedInIfTokenSaved();
        }

        public CloudParameters Parameters { get; }

        public long? Size => null;

        public Guid Id => Parameters.Id;

        public string Name => Parameters.Type.ToString();

        public DateTime Created => Parameters.Created;

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
            await _gitHub.User.Current().ConfigureAwait(false);
            Parameters.Token = password;
            Parameters.User = login;
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
                var repositories = await GetRepositories().ConfigureAwait(false);
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
            var contents = await GetRepositoryContents(details.Repository, details.Path).ConfigureAwait(false);

            return contents.Select(file => new FileModel
            {
                Name = file.Name,
                IsFolder = file.Type == "dir",
                Path = Path.Combine(details.Repository, file.Path),
                Size = file.Size
            });
        }

        public async Task<IEnumerable<FolderModel>> GetBreadCrumbs(string path)
        {
            var folderModels = new List<FolderModel>();
            var repositories = await GetRepositories().ConfigureAwait(false);
            var rootModel = new FolderModel(string.Empty, "\\", repositories.Select(repo => new FolderModel(repo.Name, repo.Name)));
            folderModels.Add(rootModel);

            var details = GetRepositoryNameAndFilePath(path);
            if (!string.IsNullOrEmpty(details.Repository))
            {
                var pathParts = details.Path.Split(new string[] { details.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                pathParts.Insert(0, details.Separator);
                for (var i = 0; i < pathParts.Count; i++)
                {
                    var subPath = pathParts[i];
                    var relativePath = Path.Combine(pathParts.Take(i + 1).ToArray());
                    var contents = await GetRepositoryContents(details.Repository, relativePath).ConfigureAwait(false);
                    var folderModel = new FolderModel(
                        details.Repository + relativePath,
                        subPath == details.Separator ? details.Repository : subPath,
                        contents
                            .Where(content => content.Type == "dir")
                            .Select(content => new FolderModel(
                                details.Repository + relativePath + details.Separator + content.Name,
                                content.Name)));
                    folderModels.Add(folderModel);
                }
            }

            return folderModels;
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

            await to.FlushAsync().ConfigureAwait(false);
            to.Close();
        }

        public Task CreateFolder(string path, string name) => throw new NotImplementedException();

        public Task RenameFile(string path, string name) => throw new NotImplementedException();

        public Task UploadFile(string to, Stream from, string name) => throw new NotImplementedException();

        public Task Delete(string path, bool isFolder) => throw new NotImplementedException();

        public void Dispose()
        {
            _httpClient.Dispose();
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

        private Task<IReadOnlyList<Repository>> GetRepositories()
        {
            var request = new RepositoryRequest
            {
                Type = RepositoryType.Owner,
                Sort = RepositorySort.Updated
            };
            return _gitHub.Repository.GetAllForCurrent(request);
        }

        private Task<IReadOnlyList<RepositoryContent>> GetRepositoryContents(string repositoryName, string path)
        {
            return _gitHub.Repository.Content.GetAllContents(_currentUserName, repositoryName, path);
        }

        private void EnsureLoggedInIfTokenSaved()
        {
            if (Parameters?.User == null || Parameters?.Token == null) return;
            _gitHub.Credentials = new Credentials(Parameters.User, Parameters.Token);
            _currentUserName = Parameters.User;
            _isAuthenticated.OnNext(true);
        }
    }
}