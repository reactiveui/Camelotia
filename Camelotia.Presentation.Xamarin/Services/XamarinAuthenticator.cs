using System;
using System.Net;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Xamarin.Services
{
    public sealed class XamarinAuthenticator : IAuthenticator
    {
        public YandexAuthenticationType YandexAuthenticationType => throw new NotImplementedException();

        public Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReceiveYandexToken(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
