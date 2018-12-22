using System;
using System.Net;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IYandexAuthenticator
    {
        YandexAuthenticationType YandexAuthenticationType { get; }

        Task<string> ReceiveYandexCode(Uri uri, IPAddress address, int port);

        Task<string> ReceiveYandexToken(Uri uri);
    }

    public enum YandexAuthenticationType
    {
        Token, Code
    }
}
