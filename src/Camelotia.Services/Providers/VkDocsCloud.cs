// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using Camelotia.Services.Apis;
using Camelotia.Services.Apis.Vk;
using Camelotia.Services.Configuration;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI.Primitives.Signals;

namespace Camelotia.Services.Providers;

/// <summary>Browses the user's VK documents. Signs in with VK ID.</summary>
[System.Diagnostics.DebuggerDisplay("VkDocsCloud: {Parameters}")]
public sealed class VkDocsCloud : ICloud, IDisposable
{
    /// <summary>The loopback port VK ID redirects back to. Register <c>http://127.0.0.1:3001/</c> as a trusted redirect URL.</summary>
    private const int LoopbackPort = 3001;

    /// <summary>Replays whether the user is signed in.</summary>
    private readonly ReplaySignal<bool> _isAuthorized = new(1);

    /// <summary>Runs the browser sign-in.</summary>
    private readonly IAuthenticator _authenticator;

    /// <summary>The VK application settings.</summary>
    private readonly VkDocsCloudOptions _options;

    /// <summary>The generated VK API client.</summary>
    private readonly IVkApi _api;

    /// <summary>The generated VK ID client.</summary>
    private readonly IVkOAuthApi _id;

    /// <summary>Initializes a new instance of the <see cref="VkDocsCloud"/> class.</summary>
    /// <param name="model">The saved parameters. <see cref="CloudParameters.User"/> holds the VK user identifier.</param>
    /// <param name="authenticator">Runs the browser sign-in.</param>
    /// <param name="options">The VK application settings.</param>
    /// <param name="api">The generated VK API client.</param>
    /// <param name="id">The generated VK ID client.</param>
    public VkDocsCloud(CloudParameters model, IAuthenticator authenticator, VkDocsCloudOptions options, IVkApi api, IVkOAuthApi id)
    {
        ArgumentNullException.ThrowIfNull(model);
        Parameters = model;
        _authenticator = authenticator;
        _options = options;
        _api = api;
        _id = id;
        _isAuthorized.OnNext(!string.IsNullOrWhiteSpace(model.Token) && !string.IsNullOrWhiteSpace(model.User));
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
    public bool CanCreateFolder => false;

    /// <inheritdoc/>
    public string InitialPath => "/";

    /// <summary>Gets the VK application identifier as the text OAuth requests send.</summary>
    private string ClientId => _options.ApplicationId.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public ValueTask HostAuth(string address, int port, string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("VK signs in through VK ID in the browser.");

    /// <inheritdoc/>
    public ValueTask DirectAuth(string login, string password, CancellationToken cancellationToken) =>
        throw new NotSupportedException("VK no longer allows password sign-in. Use VK ID in the browser.");

    /// <inheritdoc/>
    public async ValueTask OAuth(CancellationToken cancellationToken)
    {
        var verifier = Pkce.CreateRandomString();
        var state = Pkce.CreateRandomString();
        var redirect = new Uri($"http://{IPAddress.Loopback}:{LoopbackPort}/");
        var signIn = new Uri(
            $"https://id.vk.ru/authorize?response_type=code&scope=docs&client_id={ClientId}"
            + $"&redirect_uri={Uri.EscapeDataString(redirect.AbsoluteUri)}&state={state}"
            + $"&code_challenge={Pkce.CreateChallenge(verifier)}&code_challenge_method=S256");

        var received = await _authenticator.ReceiveRedirect(signIn, redirect, cancellationToken).ConfigureAwait(false);
        var query = HttpUtility.ParseQueryString(received.Query);
        if (query[nameof(state)] != state)
        {
            throw new InvalidOperationException("The sign-in reply did not match this request.");
        }

        var code = query["code"] ?? throw new InvalidOperationException(query["error_description"] ?? "VK did not return a sign-in code.");
        var deviceId = query["device_id"] ?? throw new InvalidOperationException("VK did not return a device identifier.");
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = verifier,
            ["client_id"] = ClientId,
            ["device_id"] = deviceId,
            ["redirect_uri"] = redirect.AbsoluteUri,
            ["state"] = state,
        };
        var token = await _id.ExchangeCodeAsync(form, cancellationToken).ConfigureAwait(false);

        Parameters.Token = token.AccessToken;
        Parameters.User = token.UserId.ToString(CultureInfo.InvariantCulture);
        _isAuthorized.OnNext(true);
    }

    /// <inheritdoc/>
    public async ValueTask Logout(CancellationToken cancellationToken)
    {
        if (Parameters.Token is { } token)
        {
            var form = new Dictionary<string, string> { ["client_id"] = ClientId, ["access_token"] = token };
            await _id.LogoutAsync(form, cancellationToken).ConfigureAwait(false);
        }

        Parameters.Token = null;
        Parameters.User = null;
        _isAuthorized.OnNext(false);
    }

    /// <inheritdoc/>
    public async ValueTask<IEnumerable<FileModel>> GetFiles(string path, CancellationToken cancellationToken)
    {
        var documents = await GetDocuments(cancellationToken).ConfigureAwait(false);
        var files = new List<FileModel>(documents.Count);
        foreach (var document in documents)
        {
            var id = document.Id.ToString(CultureInfo.InvariantCulture);
            files.Add(new() { Name = document.Title, Path = id, IsFolder = false, Size = document.Size, Modified = DateTimeOffset.FromUnixTimeSeconds(document.Date) });
        }

        return files;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IEnumerable<FolderModel>> GetBreadCrumbs(string path, CancellationToken cancellationToken) =>
        ValueTask.FromResult<IEnumerable<FolderModel>>([]);

    /// <inheritdoc/>
    public async ValueTask DownloadFile(string fromPath, Stream fromStream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromStream);
        var id = long.Parse(fromPath, CultureInfo.InvariantCulture);
        var documents = await GetDocuments(cancellationToken).ConfigureAwait(false);
        var document = documents.Find(d => d.Id == id) ?? throw new FileNotFoundException("The document no longer exists.", fromPath);

        var content = await _api.DownloadAsync(document.Url, cancellationToken).ConfigureAwait(false);
        await using (content.ConfigureAwait(false))
        {
            await content.CopyToAsync(fromStream, cancellationToken).ConfigureAwait(false);
        }

        await fromStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ValueTask CreateFolder(string path, string name, CancellationToken cancellationToken) =>
        throw new NotSupportedException("VK documents have no folders.");

    /// <inheritdoc/>
    public ValueTask RenameFile(string path, string name, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The VK API cannot rename documents.");

    /// <inheritdoc/>
    public async ValueTask UploadFile(string toPath, Stream fromStream, string name, CancellationToken cancellationToken)
    {
        var token = RequireToken();
        var server = (await _api.GetUploadServerAsync(token, cancellationToken).ConfigureAwait(false)).EnsureResult();
        var uploaded = await _api.UploadAsync(server.UploadUrl, new(fromStream, name), cancellationToken).ConfigureAwait(false);
        var file = uploaded.File ?? throw new VkApiException(uploaded.Error ?? $"VK did not accept {name}.");
        _ = (await _api.SaveAsync(file, name, token, cancellationToken).ConfigureAwait(false)).EnsureResult();
    }

    /// <inheritdoc/>
    public ValueTask Delete(string path, bool isFolder, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The VK API no longer deletes documents. Delete them on vk.com.");

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _isAuthorized.Dispose();

    /// <summary>Lists the signed-in user's documents.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The documents.</returns>
    /// <exception cref="InvalidOperationException">No user is signed in.</exception>
    private async ValueTask<List<VkDocument>> GetDocuments(CancellationToken cancellationToken)
    {
        var ownerId = long.Parse(Parameters.User ?? throw new InvalidOperationException("Sign in to VK first."), CultureInfo.InvariantCulture);
        var reply = await _api.GetDocumentsAsync(ownerId, RequireToken(), cancellationToken).ConfigureAwait(false);
        return reply.EnsureResult().Items;
    }

    /// <summary>Gets the saved access token.</summary>
    /// <returns>The token.</returns>
    /// <exception cref="InvalidOperationException">No user is signed in.</exception>
    private string RequireToken() => Parameters.Token ?? throw new InvalidOperationException("Sign in to VK first.");
}
