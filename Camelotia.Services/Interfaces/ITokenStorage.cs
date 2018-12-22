using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface ITokenStorage
    {
        Task WriteToken<TOwner>(string token);

        Task<string> ReadToken<TOwner>();
    }
}
