// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using Camelotia.Services.Apis;
using Camelotia.Services.Apis.Yandex;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Services.Providers;

/// <summary>Browses Yandex Disk through its REST API.</summary>
[System.Diagnostics.DebuggerDisplay("YandexDiskCloud: {Parameters}")]
public sealed class YandexDiskCloud : ICloud, IDisposable
{
    /// <summary>The number of folder entries each listing request asks for.</summary>
    private const int PageSize = 200;

    /// <summary>The loopback port Yandex redirects back to. Register <c>http://127.0.0.1:3000/</c> as the app's redirect URI.</summary>
    private const int LoopbackPort = 3000;

    /// <summary>The separator Yandex Disk paths use.</summary>
    private const char Separator = '/';

    /// <summary>The generated Disk API client.</summary>
    private readonly IYandexDiskApi _disk;

    /// <summary>The generated OAuth client.</summary>
    private readonly IYandexOAuthApi _oauth;

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthorized = new(1);

    /// <summary>Runs the browser sign-in.</summary>
    private readonly IAuthenticator _authenticator;

    /// <summary>The Yandex application settings.</summary>
    private readonly YandexDiskCloudOptions _options;

    /// <summary>Initializes a new instance of the <see cref="YandexDiskCloud"/> class.</summary>
    /// <param name="model">The saved parameters.</param>
    /// <param name="authenticator">Runs the browser sign-in.</param>
    /// <param name="options">The Yandex application settings.</param>
    /// <param name="disk">The generated Disk API client.</param>
    /// <param name="oauth">The generated OAuth client.</param>
    public YandexDiskCloud(CloudParameters model, IAuthenticator authenticator, YandexDiskCloudOptions options, IYandexDiskApi disk, IYandexOAuthApi oauth)
    {
        ArgumentNullException.ThrowIfNull(model);
        Parameters = model;
        _authenticator = authenticator;
        _options = options;
        _disk = disk;
        _oauth = oauth;
        _isAuthorized.OnNext(!string.IsNullOrWhiteSpace(model.Token));
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
    public IObservable<bool> IsAuthorized => _isAuthorized;

    /// <inheritdoc/>
    public bool SupportsDirectAuth => false;

    /// <inheritdoc/>
    public bool SupportsHostAuth => false;

    /// <inheritdoc/>
    public bool SupportsOAuth => true;

    /// <inheritdoc/>
    public bool CanCreateFolder => true;

    /// <inheritdoc/>
    public string InitialPath => "/";

    /// <inheritdoc/>
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Yandex Disk signs in through the browser.");

    /// <inheritdoc/>
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Yandex Disk signs in through the browser.");

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        var token = RequireToken();
        var diskPath = ToDiskPath(path);
        var files = new List<FileModel>();
        while (true)
        {
            var folder = await _disk.GetResourceAsync(diskPath, PageSize, files.Count, token, cancellationToken).ConfigureAwait(false);
            if (folder.Embedded is not { } page)
            {
                return files;
            }

            foreach (var item in page.Items)
            {
                files.Add(ToFileModel(item));
            }

            if (page.Items.Count == 0 || files.Count >= page.Total)
            {
                return files;
            }
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken) =>
        ValueTask.FromResult<IEnumerable<FolderModel>>([]);

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromStream);
        var token = RequireToken();
        var link = await _disk.GetDownloadLinkAsync(ToDiskPath(fromPath), token, cancellationToken).ConfigureAwait(false);
        var content = await _disk.DownloadAsync(link.Href, token, cancellationToken).ConfigureAwait(false);
        await using (content.ConfigureAwait(false))
        {
            await content.CopyToAsync(fromStream, cancellationToken).ConfigureAwait(false);
        }

        await fromStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) =>
        new(_disk.CreateFolderAsync(Combine(path, name), RequireToken(), cancellationToken));

    /// <inheritdoc/>
    public ValueTask RenameFile(string path, string name, CancellationToken cancellationToken)
    {
        var source = ToDiskPath(path);
        var parent = source[..(source.LastIndexOf(Separator) + 1)];
        return new(_disk.MoveAsync(source, parent + name, RequireToken(), cancellationToken));
    }

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        var link = await _disk.GetUploadLinkAsync(Combine(toPath, name), RequireToken(), cancellationToken).ConfigureAwait(false);
        await _disk.UploadAsync(link.Href, fromStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken) =>
        new(_disk.DeleteAsync(ToDiskPath(path), RequireToken(), cancellationToken));

    /// <inheritdoc/>
    public ValueTask Logout(CancellationToken cancellationToken)
    {
        Parameters.Token = null;
        _isAuthorized.OnNext(false);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask OAuth(CancellationToken cancellationToken)
    {
        var verifier = Pkce.CreateRandomString();
        var state = Pkce.CreateRandomString();
        var redirect = new Uri($"http://{IPAddress.Loopback}:{LoopbackPort}/");
        var signIn = new Uri(
            $"https://oauth.yandex.com/authorize?response_type=code&client_id={Uri.EscapeDataString(_options.CodeAuthClientId)}"
            + $"&redirect_uri={Uri.EscapeDataString(redirect.AbsoluteUri)}&state={state}"
            + $"&code_challenge={Pkce.CreateChallenge(verifier)}&code_challenge_method=S256");

        var received = await _authenticator.ReceiveRedirect(signIn, redirect, cancellationToken).ConfigureAwait(false);
        var query = HttpUtility.ParseQueryString(received.Query);
        if (query[nameof(state)] != state)
        {
            throw new InvalidOperationException("The sign-in reply did not match this request.");
        }

        var code = query["code"] ?? throw new InvalidOperationException(query["error_description"] ?? "Yandex did not return a sign-in code.");
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = _options.CodeAuthClientId,
            ["client_secret"] = _options.CodeAuthClientSecret,
            ["code_verifier"] = verifier,
        };
        var reply = await _oauth.ExchangeCodeAsync(form, cancellationToken).ConfigureAwait(false);

        Parameters.Token = reply.AccessToken;
        _isAuthorized.OnNext(true);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthorized.Dispose();

    /// <summary>Converts a Yandex resource into the model the browser shows.</summary>
    /// <param name="item">The resource.</param>
    /// <returns>The file model.</returns>
    private static FileModel ToFileModel(YandexResource item)
    {
        var path = item.Path.Replace("disk:", string.Empty, StringComparison.Ordinal);
        return new() { Name = item.Name, IsFolder = item.Type == "dir", Path = path, Modified = item.Modified, Size = item.Size };
    }

    /// <summary>Converts Windows separators to the Yandex Disk separator.</summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The disk path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToDiskPath(string path) => path.Replace('\\', Separator);

    /// <summary>Joins a folder path and an entry name into a disk path.</summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="name">The entry name.</param>
    /// <returns>The entry's disk path.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Combine(string folder, string name) => $"{ToDiskPath(folder).TrimEnd(Separator)}{Separator}{name}";

    /// <summary>Gets the saved access token.</summary>
    /// <returns>The token.</returns>
    /// <exception cref="InvalidOperationException">No user is signed in.</exception>
    private string RequireToken() => Parameters.Token ?? throw new InvalidOperationException("Sign in to Yandex Disk first.");
}
