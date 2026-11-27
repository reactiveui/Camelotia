// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Util.Store;

namespace Camelotia.Services.Providers;

/// <summary>Signs the user in to Google and returns the credential that authorizes Drive requests.</summary>
/// <param name="secrets">The OAuth client identifier and secret.</param>
/// <param name="user">The local user name the credential is stored under.</param>
/// <param name="store">The store that keeps the credential between runs.</param>
/// <param name="cancellationToken">Cancels the sign-in.</param>
/// <returns>The credential.</returns>
public delegate Task<IConfigurableHttpClientInitializer> GoogleDriveAuthorizer(ClientSecrets secrets, string user, IDataStore store, CancellationToken cancellationToken);
