// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Answers HTTP requests from a table of routes, so a real client library runs without a network.</summary>
[System.Diagnostics.DebuggerDisplay("StubHttpHandler: {Requests.Count} requests")]
public sealed class StubHttpHandler : HttpMessageHandler
{
    /// <summary>The routes, checked in the order they were added.</summary>
    private readonly List<(HttpMethod Method, string PathPrefix, Func<HttpRequestMessage, HttpResponseMessage> Respond)> _routes = [];

    /// <summary>Gets the requests the handler received, in order.</summary>
    public List<HttpRequestMessage> Requests { get; } = [];

    /// <summary>Gets the request bodies, in the same order as <see cref="Requests"/>.</summary>
    public List<string> Bodies { get; } = [];

    /// <summary>Creates a successful JSON reply.</summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The reply.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HttpResponseMessage Json(string json) => Json(json, HttpStatusCode.OK);

    /// <summary>Creates a JSON reply.</summary>
    /// <param name="json">The JSON text.</param>
    /// <param name="status">The status code.</param>
    /// <returns>The reply.</returns>
    public static HttpResponseMessage Json(string json, HttpStatusCode status) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    /// <summary>Answers requests whose method matches and whose path starts with a prefix.</summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="pathPrefix">The start of the request path and query.</param>
    /// <param name="respond">Creates the reply.</param>
    /// <returns>This handler, for chaining.</returns>
    public StubHttpHandler On(HttpMethod method, string pathPrefix, Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _routes.Add((method, pathPrefix, respond));
        return this;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Requests.Add(request);
        Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        var path = request.RequestUri!.PathAndQuery;
        foreach (var route in _routes)
        {
            if (route.Method == request.Method && path.StartsWith(route.PathPrefix, StringComparison.Ordinal))
            {
                return route.Respond(request);
            }
        }

        return new(HttpStatusCode.NotFound) { RequestMessage = request };
    }
}
