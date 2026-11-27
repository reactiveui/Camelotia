// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Google.Apis.Http;

namespace Camelotia.Tests.Fakes;

/// <summary>A Google credential that adds nothing to the requests, for tests that stub the Drive replies.</summary>
[System.Diagnostics.DebuggerDisplay("FakeGoogleCredential")]
public sealed class FakeGoogleCredential : IConfigurableHttpClientInitializer
{
    /// <summary>Gets the number of HTTP clients the credential was asked to configure.</summary>
    public int InitializedClients { get; private set; }

    /// <inheritdoc/>
    public void Initialize(ConfigurableHttpClient httpClient) => InitializedClients++;
}
