// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Refit;

namespace Camelotia.Services.Apis.Yandex;

/// <summary>The Yandex OAuth server at <c>https://oauth.yandex.com</c>.</summary>
public interface IYandexOAuthApi
{
    /// <summary>Exchanges an authorization code for an access token.</summary>
    /// <param name="form">The form fields: grant_type, code, client_id, client_secret and code_verifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The token reply.</returns>
    [Post("/token")]
    ValueTask<YandexToken> ExchangeCodeAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form, CancellationToken cancellationToken);
}
