using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface ITokenStorage
    {
        Task WriteToken(AuthenticationTokenOwner owner, string token);

        Task<string> ReadToken(AuthenticationTokenOwner owner);
    }

    public enum AuthenticationTokenOwner
    {
        Vkontakte, Yandex
    }
}
