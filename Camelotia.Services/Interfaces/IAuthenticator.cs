using System;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IAuthenticator
    {
        GrantType GrantType { get; }

        Task<string> ReceiveCode(Uri uri, Uri returnUri);

        Task<string> ReceiveToken(Uri uri);
    }

    public enum GrantType
    {
        AccessToken,
        AuthorizationCode
    }
}
