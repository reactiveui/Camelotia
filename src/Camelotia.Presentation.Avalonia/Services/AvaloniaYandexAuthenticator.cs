using System.Diagnostics;
using System.Net;
using System.Text;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services;

public sealed class AvaloniaYandexAuthenticator : IAuthenticator
{
    private const string SuccessContent = "<html><body>Please return to the app.</body></html>";

    public GrantType GrantType => GrantType.AuthorizationCode;

    public Task<string> ReceiveToken(Uri uri) => throw new PlatformNotSupportedException();

    public async Task<string> ReceiveCode(Uri uri, Uri returnUrl)
    {
        var server = returnUrl.ToString();
        var listener = new HttpListener();
        listener.Prefixes.Add(server);
        listener.Start();
        new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = uri.ToString()
                }
            }
            .Start();

        var context = await listener.GetContextAsync().ConfigureAwait(false);
        var code = context.Request.QueryString["code"];

        var buffer = Encoding.UTF8.GetBytes(SuccessContent);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

        context.Response.Close();
        listener.Close();
        return code;
    }
}
