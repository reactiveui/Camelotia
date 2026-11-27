// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Primitives.Signals;
using Renci.SshNet;

namespace Camelotia.Services.Providers;

/// <summary>Browses an SFTP server. Each operation opens its own connection.</summary>
[System.Diagnostics.DebuggerDisplay("SftpCloud: {Parameters}")]
public sealed class SftpCloud : ICloud, IDisposable
{
    /// <summary>The separator SFTP paths use.</summary>
    private const char Separator = '/';

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthorized = new(1);

    /// <summary>Creates SFTP clients.</summary>
    private readonly SftpClientFactory _clientFactory;

    /// <summary>Creates a client for the signed-in server, or <see langword="null"/> when signed out.</summary>
    private Func<ISftpClient>? _factory;

    /// <summary>Initializes a new instance of the <see cref="SftpCloud"/> class that connects with SSH.NET.</summary>
    /// <param name="model">The saved parameters.</param>
    public SftpCloud(CloudParameters model)
        : this(model, CreateSshNetClient)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SftpCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="clientFactory">Creates the SFTP client for each operation.</param>
    public SftpCloud(CloudParameters model, SftpClientFactory clientFactory)
    {
        Parameters = model;
        _clientFactory = clientFactory;
        _isAuthorized.OnNext(false);
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
    public string InitialPath => "/";

    /// <inheritdoc/>
    public IObservable<bool> IsAuthorized => _isAuthorized;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => false;

    /// <inheritdoc/>
    public bool SupportsHostAuth => true;

    /// <inheritdoc/>
    public bool SupportsOAuth => false;

    /// <inheritdoc/>
    public bool CanCreateFolder => true;

    /// <inheritdoc/>
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("SFTP signs in with a host, user name and password.");

    /// <inheritdoc/>
    public ValueTask OAuth(CancellationToken cancellationToken) =>
        throw new NotSupportedException("SFTP signs in with a host, user name and password.");

    /// <inheritdoc/>
    public async ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken)
    {
        _factory = () => _clientFactory(address, port, login, password);
        _ = await GetFiles(InitialPath, cancellationToken).ConfigureAwait(false);
        _isAuthorized.OnNext(true);
    }

    /// <inheritdoc/>
    public ValueTask Logout(CancellationToken cancellationToken)
    {
        _factory = null;
        _isAuthorized.OnNext(false);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        var files = new List<FileModel>();
        await foreach (var item in connection.ListDirectoryAsync(ToServerPath(path), cancellationToken).ConfigureAwait(false))
        {
            if (item.Name is "." or "..")
            {
                continue;
            }

            files.Add(new() { Name = item.Name, Path = item.FullName, IsFolder = item.IsDirectory, Modified = item.LastWriteTimeUtc, Size = item.Length });
        }

        return files;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        var trail = new List<FolderModel>();
        var current = string.Empty;
        var segments = ToServerPath(path).Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        for (var i = -1; i < segments.Length; i++)
        {
            current = i < 0 ? Separator.ToString() : current.TrimEnd(Separator) + Separator + segments[i];
            var children = new List<FolderModel>();
            await foreach (var item in connection.ListDirectoryAsync(current, cancellationToken).ConfigureAwait(false))
            {
                if (item.IsDirectory && item.Name is not ("." or ".."))
                {
                    children.Add(new(item.FullName, item.Name));
                }
            }

            trail.Add(new(current, i < 0 ? Separator.ToString() : segments[i], children));
        }

        return trail;
    }

    /// <inheritdoc/>
    public async ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        await connection.CreateDirectoryAsync(Combine(path, name), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask RenameFile(string path, string name, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        var source = ToServerPath(path);
        var index = source.TrimEnd(Separator).LastIndexOf(Separator);
        var parent = index <= 0 ? Separator.ToString() : source[..index];
        await connection.RenameFileAsync(source, Combine(parent, name), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        if (isFolder)
        {
            await connection.DeleteDirectoryAsync(ToServerPath(path), cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await connection.DeleteFileAsync(ToServerPath(path), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        await connection.UploadFileAsync(fromStream, Combine(toPath, name), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        await connection.DownloadFileAsync(ToServerPath(fromPath), fromStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthorized.Dispose();

    /// <summary>Converts Windows separators to the SFTP separator.</summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The server path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToServerPath(string path) => path.Replace('\\', Separator);

    /// <summary>Creates an SSH.NET client.</summary>
    /// <param name="host">The server address.</param>
    /// <param name="port">The server port.</param>
    /// <param name="user">The user name.</param>
    /// <param name="password">The password.</param>
    /// <returns>The unconnected client.</returns>
    /// <remarks>The sample trusts any host key. A real client compares the key with one the user confirmed earlier.</remarks>
    private static SftpClient CreateSshNetClient(string host, int port, string user, string password)
    {
        var client = new SftpClient(host, port, user, password);
        client.HostKeyReceived += static (_, args) => args.CanTrust = true;
        return client;
    }

    /// <summary>Joins a folder path and an entry name.</summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>The entry path.</returns>
    private static string Combine(string folder, string name) => ToServerPath(folder).TrimEnd(Separator) + Separator + name;

    /// <summary>Opens a connection to the signed-in server.</summary>
    /// <param name="cancellationToken">Cancels the connection attempt.</param>
    /// <returns>The connected client, which the caller disposes.</returns>
    /// <exception cref="InvalidOperationException">No server is signed in.</exception>
    private async ValueTask<ISftpClient> Connect(CancellationToken cancellationToken)
    {
        var connection = (_factory ?? throw new InvalidOperationException("Sign in to the SFTP server first."))();
        try
        {
            await connection.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
