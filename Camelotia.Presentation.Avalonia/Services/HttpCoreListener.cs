using System.Net;
using System.Text;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class HttpCoreListener  : IListener
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";
        private HttpListener _listener;
        
        public void Start(IPAddress address, int port)
        {
            var server = $"http://{address}:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add(server);
            _listener.Start();
        }

        public async Task<string> GetCode()
        {
            var context = await _listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            
            var buffer = Encoding.UTF8.GetBytes(SuccessContent);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            
            context.Response.Close();
            return code;
        }

        public void Stop() => _listener.Close();
    }
}