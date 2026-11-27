// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Web;
using Camelotia.Services.Interfaces;

namespace Camelotia.Tests.Fakes;

/// <summary>An <see cref="IAuthenticator"/> that answers the redirect the test describes, echoing the sign-in state.</summary>
[System.Diagnostics.DebuggerDisplay("FakeAuthenticator")]
public sealed class FakeAuthenticator : IAuthenticator
{
    /// <summary>Gets or sets the query the redirect carries, without the state; <c>{state}</c> is replaced with the request's state.</summary>
    public string Query { get; set; } = "code=the-code&state={state}&device_id=the-device";

    /// <summary>Gets the sign-in pages the fake was asked to open.</summary>
    public List<Uri> SignInPages { get; } = [];

    /// <inheritdoc/>
    public ValueTask<Uri> ReceiveRedirect(Uri signInUri, Uri redirectUri, CancellationToken cancellationToken)
    {
        SignInPages.Add(signInUri);
        var state = HttpUtility.ParseQueryString(signInUri.Query)["state"];
        return ValueTask.FromResult(new Uri($"{redirectUri.AbsoluteUri}?{Query.Replace("{state}", state, StringComparison.Ordinal)}"));
    }
}
