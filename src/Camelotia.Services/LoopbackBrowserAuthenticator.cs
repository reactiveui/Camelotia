// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics;
using System.Net;
using Camelotia.Services.Interfaces;

namespace Camelotia.Services;

/// <summary>Opens the sign-in page in the default browser and listens on a loopback address for the redirect.</summary>
[DebuggerDisplay("LoopbackBrowserAuthenticator")]
public sealed class LoopbackBrowserAuthenticator : IAuthenticator
{
    /// <summary>The page the browser shows once the redirect arrives.</summary>
    private static readonly byte[] ReturnPage = "<html><body>Sign-in finished. Please return to Camelotia.</body></html>"u8.ToArray();

    /// <summary>Opens an address in a browser.</summary>
    private readonly Action<Uri> _openBrowser;

    /// <summary>Initializes a new instance of the <see cref="LoopbackBrowserAuthenticator"/> class that opens the desktop's default browser.</summary>
    public LoopbackBrowserAuthenticator()
        : this(OpenBrowser)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="LoopbackBrowserAuthenticator"/> class.</summary>
    /// <param name="openBrowser">Opens the sign-in page in a browser.</param>
    public LoopbackBrowserAuthenticator(Action<Uri> openBrowser) => _openBrowser = openBrowser;

    /// <inheritdoc/>
    public async ValueTask<Uri> ReceiveRedirect(Uri signInUri, Uri redirectUri, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(signInUri);
        ArgumentNullException.ThrowIfNull(redirectUri);
        if (signInUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Sign-in pages must use https.", nameof(signInUri));
        }

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri.GetLeftPart(UriPartial.Path));
        listener.Start();
        _openBrowser(signInUri);
        cancellationToken.ThrowIfCancellationRequested();

        HttpListenerContext context;

        // Stopping the listener ends the pending wait, so no task is left running after a cancellation.
        await using (cancellationToken.Register(listener.Stop).ConfigureAwait(false))
        {
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception exception) when (cancellationToken.IsCancellationRequested && exception is HttpListenerException or ObjectDisposedException)
            {
                throw new OperationCanceledException("The sign-in was cancelled.", exception, cancellationToken);
            }
        }

        var received = context.Request.Url ?? throw new InvalidOperationException("The redirect arrived without an address.");

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = ReturnPage.Length;
        await context.Response.OutputStream.WriteAsync(ReturnPage, cancellationToken).ConfigureAwait(false);
        context.Response.Close();
        return received;
    }

    /// <summary>Gets the program that opens a URL in the default browser.</summary>
    /// <param name="isWindows">Whether the app runs on Windows.</param>
    /// <param name="isMacOS">Whether the app runs on macOS.</param>
    /// <returns>The program name: explorer on Windows, open on macOS, and xdg-open on Linux and other Unix systems.</returns>
    internal static string GetUrlOpener(bool isWindows, bool isMacOS)
    {
        if (isWindows)
        {
            return "explorer.exe";
        }

        return isMacOS ? "open" : "xdg-open";
    }

    /// <summary>Starts the platform's URL opener with the address as one argument, so no shell parses it.</summary>
    /// <param name="address">The https address to open.</param>
    /// <exception cref="InvalidOperationException">The URL opener did not start.</exception>
    private static void OpenBrowser(Uri address)
    {
        var start = new ProcessStartInfo(GetUrlOpener(OperatingSystem.IsWindows(), OperatingSystem.IsMacOS())) { UseShellExecute = false };
        start.ArgumentList.Add(address.AbsoluteUri);
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"'{start.FileName}' did not start.");
    }
}
