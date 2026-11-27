// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Interfaces;

/// <summary>Runs the browser part of an OAuth sign-in.</summary>
public interface IAuthenticator
{
    /// <summary>Opens the sign-in page and waits for the provider to redirect back.</summary>
    /// <param name="signInUri">The provider's sign-in page.</param>
    /// <param name="redirectUri">The address the provider redirects back to.</param>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>The full redirect address, with the query values the provider added.</returns>
    ValueTask<Uri> ReceiveRedirect(Uri signInUri, Uri redirectUri, CancellationToken cancellationToken);
}
