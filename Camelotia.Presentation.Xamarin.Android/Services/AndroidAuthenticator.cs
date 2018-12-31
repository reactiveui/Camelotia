using System;
using System.Net;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Xamarin.Droid.Services
{
    public sealed class AndroidAuthenticator : IAuthenticator
    {
        public YandexAuthenticationType YandexAuthenticationType => YandexAuthenticationType.Token;

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port) => throw new PlatformNotSupportedException();

        public Task<string> ReceiveYandexToken(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
