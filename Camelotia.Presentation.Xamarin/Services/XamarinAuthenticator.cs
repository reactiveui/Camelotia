using System;
using System.Net;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Xamarin.Services
{
    public sealed class XamarinAuthenticator : IAuthenticator
    {
        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Token;

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port) => throw new PlatformNotSupportedException();

        public Task<string> ReceiveYandexToken(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
