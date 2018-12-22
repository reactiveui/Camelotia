using Camelotia.Services.Interfaces;
using System.Threading.Tasks;
using System.Net;
using System;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalWindowsYandexAuthenticator : IYandexAuthenticator
    {
        private const string SuccessContent = "<html><body>Please return to the app.</body></html>";

        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Token;

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port) => throw new PlatformNotSupportedException();

        public Task<string> ReceiveYandexToken(Uri uri)
        {
            // TODO

            throw new NotImplementedException();
        }
    }
}
