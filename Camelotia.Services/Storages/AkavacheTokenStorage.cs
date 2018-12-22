using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using System.Reactive.Linq;
using Akavache;

namespace Camelotia.Services.Storages
{
    public sealed class AkavacheTokenStorage : ITokenStorage
    {
        public async Task<string> ReadToken(AuthenticationTokenOwner owner)
        {
            var key = owner.ToString();
            var token = await BlobCache.Secure.GetObject<string>(key);
            return token;
        }

        public async Task WriteToken(AuthenticationTokenOwner owner, string token)
        {
            var key = owner.ToString();
            await BlobCache.Secure.InsertObject(key, token);
        }
    }
}
