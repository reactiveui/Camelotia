// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using FluentFTP;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Services.Providers;

/// <summary>Browses an FTP server. Each operation opens its own connection.</summary>
[System.Diagnostics.DebuggerDisplay("FtpCloud: {Parameters}")]
public sealed class FtpCloud : ICloud, IDisposable
{
    /// <summary>The separator FTP paths use.</summary>
    private const char Separator = '/';

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthorized = new(1);

    /// <summary>Creates FTP clients.</summary>
    private readonly FtpClientFactory _clientFactory;

    /// <summary>Creates a client for the signed-in server, or <see langword="null"/> when signed out.</summary>
    private Func<IAsyncFtpClient>? _factory;

    /// <summary>Initializes a new instance of the <see cref="FtpCloud"/> class that connects with FluentFTP.</summary>
    /// <param name="model">The saved parameters.</param>
    public FtpCloud(CloudParameters model)
        : this(model, static (host, port, user, password) => new AsyncFtpClient(host, user, password, port))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FtpCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="clientFactory">Creates the FTP client for each operation.</param>
    public FtpCloud(CloudParameters model, FtpClientFactory clientFactory)
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
    public string InitialPath => "/";

    /// <inheritdoc/>
    public string Name => Parameters.Type.ToString();

    /// <inheritdoc/>
    public DateTimeOffset Created => Parameters.Created;

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
    public ValueTask OAuth(CancellationToken cancellationToken) =>
        throw new NotSupportedException("FTP signs in with a host, user name and password.");

    /// <inheritdoc/>
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("FTP signs in with a host, user name and password.");

    /// <inheritdoc/>
    public async ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken)
    {
        _factory = () => _clientFactory(address, port, login, password);
        _ = await GetFiles(InitialPath, cancellationToken).ConfigureAwait(false);
        _isAuthorized.OnNext(true);
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        var listing = await connection.GetListing(path, cancellationToken).ConfigureAwait(false);
        var files = new List<FileModel>(listing.Length);
        foreach (var item in listing)
        {
            files.Add(new()
            {
                IsFolder = item.Type == FtpObjectType.Directory,
                Modified = item.Modified == DateTime.MinValue ? null : new DateTimeOffset(DateTime.SpecifyKind(item.Modified, DateTimeKind.Utc)),
                Name = item.Name,
                Path = item.FullName,
                Size = item.Size,
            });
        }

        return files;
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        var trail = new List<FolderModel>();
        var current = string.Empty;
        var segments = path.Split(['\\', Separator], StringSplitOptions.RemoveEmptyEntries);
        for (var i = -1; i < segments.Length; i++)
        {
            current = i < 0 ? Separator.ToString() : current.TrimEnd(Separator) + Separator + segments[i];
            var children = new List<FolderModel>();
            foreach (var item in await connection.GetListing(current, cancellationToken).ConfigureAwait(false))
            {
                if (item.Type == FtpObjectType.Directory)
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
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        _ = await connection.CreateDirectory(Combine(path, name), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask RenameFile(string path, string name, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        await connection.Rename(path, Combine(GetParent(path), name), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        if (isFolder)
        {
            await connection.DeleteDirectory(path, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await connection.DeleteFile(path, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public ValueTask Logout(CancellationToken cancellationToken)
    {
        _factory = null;
        _isAuthorized.OnNext(false);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        _ = await connection.UploadStream(fromStream, Combine(toPath, name), token: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        await using var connection = await Connect(cancellationToken).ConfigureAwait(false);
        if (!await connection.DownloadStream(fromStream, fromPath, token: cancellationToken).ConfigureAwait(false))
        {
            throw new IOException($"The FTP server did not send {fromPath}.");
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthorized.Dispose();

    /// <summary>Joins a folder path and an entry name.</summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>The entry path.</returns>
    private static string Combine(string folder, string name) => folder.TrimEnd(Separator) + Separator + name;

    /// <summary>Gets the folder that holds an entry.</summary>
    /// <param name="path">The entry path.</param>
    /// <returns>The parent folder path, or the root.</returns>
    private static string GetParent(string path)
    {
        var index = path.TrimEnd(Separator).LastIndexOf(Separator);
        return index <= 0 ? Separator.ToString() : path[..index];
    }

    /// <summary>Opens a connection to the signed-in server.</summary>
    /// <param name="cancellationToken">Cancels the connection attempt.</param>
    /// <returns>The connected client, which the caller disposes.</returns>
    /// <exception cref="InvalidOperationException">No server is signed in.</exception>
    private async ValueTask<IAsyncFtpClient> Connect(CancellationToken cancellationToken)
    {
        var connection = (_factory ?? throw new InvalidOperationException("Sign in to the FTP server first."))();
        try
        {
            await connection.Connect(cancellationToken).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
