// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Refit;

namespace Camelotia.Services.Apis.Vk;

/// <summary>The VK ID OAuth server at <c>https://id.vk.ru</c>.</summary>
public interface IVkOAuthApi
{
    /// <summary>Exchanges an authorization code for tokens.</summary>
    /// <param name="form">The form fields: grant_type, code, code_verifier, client_id, device_id, redirect_uri and state.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The token reply.</returns>
    [Post("/oauth2/auth")]
    ValueTask<VkToken> ExchangeCodeAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form, CancellationToken cancellationToken);

    /// <summary>Revokes an access token.</summary>
    /// <param name="form">The form fields: client_id and access_token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when VK has revoked the token.</returns>
    [Post("/oauth2/logout")]
    Task LogoutAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form, CancellationToken cancellationToken);
}
