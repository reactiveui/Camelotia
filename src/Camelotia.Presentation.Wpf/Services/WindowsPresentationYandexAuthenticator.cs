using Camelotia.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Camelotia.Presentation.Wpf.Services
{
    public sealed class WindowsPresentationYandexAuthenticator : IAuthenticator
    {
        public GrantType GrantType => throw new NotImplementedException();

        public Task<string> ReceiveCode(Uri uri, Uri returnUri)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReceiveToken(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
