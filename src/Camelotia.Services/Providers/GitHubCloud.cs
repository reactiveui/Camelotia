// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Octokit;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Services.Providers;

/// <summary>Browses the signed-in user's GitHub repositories. Paths have the form <c>repository/folder/file</c>.</summary>
[System.Diagnostics.DebuggerDisplay("GitHubCloud: {Parameters}")]
public sealed class GitHubCloud : ICloud, IDisposable
{
    /// <summary>The separator between the repository name and the path inside it.</summary>
    private const char Separator = '/';

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthenticated = new(1);

    /// <summary>The Octokit client.</summary>
    private readonly IGitHubClient _gitHub;

    /// <summary>Initializes a new instance of the <see cref="GitHubCloud"/> class that calls github.com.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="options">The GitHub application settings.</param>
    public GitHubCloud(CloudParameters model, GitHubCloudOptions options)
        : this(model, new GitHubClient(new ProductHeaderValue((options ?? throw new ArgumentNullException(nameof(options))).GithubApplicationId)))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GitHubCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="gitHub">The Octokit client.</param>
    public GitHubCloud(CloudParameters model, IGitHubClient gitHub)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(gitHub);
        Parameters = model;
        _gitHub = gitHub;
        var signedIn = model is { User: not null, Token: not null };
        if (signedIn)
        {
            _gitHub.Connection.Credentials = new(model.Token);
        }

        _isAuthenticated.OnNext(signedIn);
    }

    /// <inheritdoc/>
    public CloudParameters Parameters { get; }

    /// <inheritdoc/>
    public long? Size => null;

    /// <inheritdoc/>
    public Guid Id => Parameters.Id;

    /// <inheritdoc/>
    public string Name => Parameters.Type.ToString();

    /// <inheritdoc/>
    public DateTimeOffset Created => Parameters.Created;

    /// <inheritdoc/>
    public string InitialPath => string.Empty;

    /// <inheritdoc/>
    public IObservable<bool> IsAuthorized => _isAuthenticated;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => true;

    /// <inheritdoc/>
    public bool SupportsHostAuth => false;

    /// <inheritdoc/>
    public bool SupportsOAuth => false;

    /// <inheritdoc/>
    public bool CanCreateFolder => false;

    /// <inheritdoc/>
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("GitHub signs in with a personal access token.");

    /// <inheritdoc/>
    public ValueTask OAuth(CancellationToken cancellationToken) =>
        throw new NotSupportedException("GitHub signs in with a personal access token.");

    /// <summary>Signs in with a personal access token. GitHub no longer accepts account passwords for the API.</summary>
    /// <param name="login">The user name. GitHub's reply replaces it with the account's login.</param>
    /// <param name="password">A personal access token with the <c>repo</c> scope.</param>
    /// <param name="cancellationToken">Cancels the sign-in.</param>
    /// <returns>A task that completes when the user is signed in.</returns>
    public async ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken)
    {
        _gitHub.Connection.Credentials = new(password);
        var user = await _gitHub.User.Current().WaitAsync(cancellationToken).ConfigureAwait(false);
        Parameters.Token = password;
        Parameters.User = user.Login;
        _isAuthenticated.OnNext(true);
    }

    /// <inheritdoc/>
    public ValueTask Logout(CancellationToken cancellationToken)
    {
        _gitHub.Connection.Credentials = Credentials.Anonymous;
        Parameters.Token = null;
        Parameters.User = null;
        _isAuthenticated.OnNext(false);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        var files = new List<FileModel>();
        if (string.IsNullOrWhiteSpace(path))
        {
            foreach (var repository in await GetRepositories(cancellationToken).ConfigureAwait(false))
            {
                files.Add(new() { IsFolder = true, Modified = repository.UpdatedAt, Name = repository.Name, Path = repository.Name, Size = repository.Size });
            }

            return files;
        }

        var (repositoryName, filePath) = Split(path);
        foreach (var content in await GetContents(repositoryName, filePath, cancellationToken).ConfigureAwait(false))
        {
            var isFolder = content.Type == ContentType.Dir;
            files.Add(new() { Name = content.Name, IsFolder = isFolder, Path = repositoryName + Separator + content.Path, Size = content.Size });
        }

        return files;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken)
    {
        var repositories = new List<FolderModel>();
        foreach (var repository in await GetRepositories(cancellationToken).ConfigureAwait(false))
        {
            repositories.Add(new(repository.Name, repository.Name));
        }

        var trail = new List<FolderModel> { new(string.Empty, Separator.ToString(), repositories) };
        if (string.IsNullOrWhiteSpace(path))
        {
            return trail;
        }

        var (repositoryName, filePath) = Split(path);
        trail.Add(new(repositoryName, repositoryName, await GetSubfolders(repositoryName, string.Empty, cancellationToken).ConfigureAwait(false)));

        var current = string.Empty;
        foreach (var segment in filePath.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
        {
            current = current.Length == 0 ? segment : current + Separator + segment;
            var children = await GetSubfolders(repositoryName, current, cancellationToken).ConfigureAwait(false);
            trail.Add(new(repositoryName + Separator + current, segment, children));
        }

        return trail;
    }

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromStream);
        var (repositoryName, filePath) = Split(fromPath);
        var bytes = await _gitHub.Repository.Content.GetRawContent(RequireUser(), repositoryName, filePath).WaitAsync(cancellationToken).ConfigureAwait(false);
        await fromStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await fromStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromStream);
        var (repositoryName, folder) = Split(toPath);
        var filePath = folder.Length == 0 ? name : folder + Separator + name;
        var buffer = new MemoryStream();
        await using (buffer.ConfigureAwait(false))
        {
            await fromStream.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        var request = new CreateFileRequest($"Upload {name}", Convert.ToBase64String(buffer.ToArray()), convertContentToBase64: false);
        _ = await _gitHub.Repository.Content.CreateFile(RequireUser(), repositoryName, filePath, request).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken)
    {
        if (isFolder)
        {
            throw new NotSupportedException("Delete the files in a GitHub folder one at a time.");
        }

        var (repositoryName, filePath) = Split(path);
        var contents = await GetContents(repositoryName, filePath, cancellationToken).ConfigureAwait(false);
        var request = new DeleteFileRequest($"Delete {filePath}", contents[0].Sha);
        await _gitHub.Repository.Content.DeleteFile(RequireUser(), repositoryName, filePath, request).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Git does not store empty folders.");

    /// <inheritdoc/>
    public ValueTask RenameFile(string path, string name, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The GitHub contents API cannot rename files.");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthenticated.Dispose();

    /// <summary>Splits a Camelotia path into the repository name and the path inside the repository.</summary>
    /// <param name="path">The path, such as <c>repository/folder/file</c>.</param>
    /// <returns>The repository name, and the path inside it, which is empty at the repository root.</returns>
    private static (string Repository, string Path) Split(string path)
    {
        var trimmed = path.Replace('\\', Separator).Trim(Separator);
        var index = trimmed.IndexOf(Separator, StringComparison.Ordinal);
        return index < 0 ? (trimmed, string.Empty) : (trimmed[..index], trimmed[(index + 1)..]);
    }

    /// <summary>Lists the folders directly inside a repository folder.</summary>
    /// <param name="repositoryName">The repository name.</param>
    /// <param name="path">The folder inside the repository, or empty for the root.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The subfolders.</returns>
    private async ValueTask<List<FolderModel>> GetSubfolders(string repositoryName, string path, CancellationToken cancellationToken)
    {
        var folders = new List<FolderModel>();
        foreach (var content in await GetContents(repositoryName, path, cancellationToken).ConfigureAwait(false))
        {
            if (content.Type == ContentType.Dir)
            {
                folders.Add(new(repositoryName + Separator + content.Path, content.Name));
            }
        }

        return folders;
    }

    /// <summary>Lists the repositories the signed-in user owns, most recently updated first.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The repositories.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<IReadOnlyList<Repository>> GetRepositories(CancellationToken cancellationToken) =>
        _gitHub.Repository
            .GetAllForCurrent(new RepositoryRequest { Type = RepositoryType.Owner, Sort = RepositorySort.Updated })
            .WaitAsync(cancellationToken);

    /// <summary>Lists the entries in a repository folder.</summary>
    /// <param name="repositoryName">The repository name.</param>
    /// <param name="path">The folder or file inside the repository, or empty for the root.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The entries.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<IReadOnlyList<RepositoryContent>> GetContents(string repositoryName, string path, CancellationToken cancellationToken) =>
        (path.Length == 0
            ? _gitHub.Repository.Content.GetAllContents(RequireUser(), repositoryName)
            : _gitHub.Repository.Content.GetAllContents(RequireUser(), repositoryName, path))
        .WaitAsync(cancellationToken);

    /// <summary>Gets the signed-in user's login.</summary>
    /// <returns>The login.</returns>
    /// <exception cref="InvalidOperationException">No user is signed in.</exception>
    private string RequireUser() => Parameters.User ?? throw new InvalidOperationException("Sign in to GitHub first.");
}
