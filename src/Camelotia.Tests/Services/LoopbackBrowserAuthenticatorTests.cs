// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using System.Net.Sockets;
using Camelotia.Services;

namespace Camelotia.Tests.Services;

/// <summary>Tests <see cref="LoopbackBrowserAuthenticator"/> with a test standing in for the browser.</summary>
public sealed class LoopbackBrowserAuthenticatorTests
{
    /// <summary>Stands in for the browser that follows the redirect.</summary>
    private static readonly HttpClient Browser = new();

    /// <summary>Receives the redirect the browser makes to the loopback address, and answers it with the return page.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task ReceiveRedirect_ReturnsTheRedirectAddress()
    {
        var redirect = new Uri($"http://{IPAddress.Loopback}:{GetFreePort()}/");
        Task<string>? page = null;
        Uri? opened = null;
        var authenticator = new LoopbackBrowserAuthenticator(signIn =>
        {
            opened = signIn;
            page = Browser.GetStringAsync(new Uri(redirect, "?code=abc&state=xyz"));
        });

        var received = await authenticator.ReceiveRedirect(new("https://sign-in.example/authorize"), redirect, CancellationToken.None);

        await Assert.That(opened!.Host).IsEqualTo("sign-in.example");
        await Assert.That(received.Query).IsEqualTo("?code=abc&state=xyz");
        await Assert.That(await page!).Contains("Sign-in finished");
    }

    /// <summary>Stops waiting when the sign-in is cancelled.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task ReceiveRedirect_StopsWhenCancelled()
    {
        using var cancellation = new CancellationTokenSource();
        var authenticator = new LoopbackBrowserAuthenticator(_ => cancellation.Cancel());
        var redirect = new Uri($"http://{IPAddress.Loopback}:{GetFreePort()}/");

        _ = await Assert.That(async () => await authenticator.ReceiveRedirect(new("https://sign-in.example/"), redirect, cancellation.Token)).Throws<OperationCanceledException>();
    }

    /// <summary>Refuses a sign-in page that is not https, and missing addresses.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task ReceiveRedirect_RejectsBadAddresses()
    {
        var authenticator = new LoopbackBrowserAuthenticator(static _ => { });
        var redirect = new Uri("http://127.0.0.1:1/");

        _ = await Assert.That(async () => await authenticator.ReceiveRedirect(new("http://sign-in.example/"), redirect, CancellationToken.None)).Throws<ArgumentException>();
        _ = await Assert.That(async () => await authenticator.ReceiveRedirect(null!, redirect, CancellationToken.None)).Throws<ArgumentNullException>();
        _ = await Assert.That(async () => await authenticator.ReceiveRedirect(redirect, null!, CancellationToken.None)).Throws<ArgumentNullException>();
    }

    /// <summary>Picks the platform's program for opening a URL in the default browser.</summary>
    /// <param name="isWindows">Whether the app runs on Windows.</param>
    /// <param name="isMacOS">Whether the app runs on macOS.</param>
    /// <param name="expected">The expected program.</param>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    [Arguments(true, false, "explorer.exe")]
    [Arguments(false, true, "open")]
    [Arguments(false, false, "xdg-open")]
    public async Task GetUrlOpener_PicksThePlatformOpener(bool isWindows, bool isMacOS, string expected) =>
        await Assert.That(LoopbackBrowserAuthenticator.GetUrlOpener(isWindows, isMacOS)).IsEqualTo(expected);

    /// <summary>Finds a loopback port nothing is listening on.</summary>
    /// <returns>The port.</returns>
    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
