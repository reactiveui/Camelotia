// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Google.Apis.Http;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Makes Google's client library send its requests to a stub handler.</summary>
/// <param name="handler">The stub handler.</param>
[System.Diagnostics.DebuggerDisplay("StubGoogleHttpClientFactory")]
public sealed class StubGoogleHttpClientFactory(StubHttpHandler handler) : HttpClientFactory
{
    /// <inheritdoc/>
    protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args) => handler;
}
