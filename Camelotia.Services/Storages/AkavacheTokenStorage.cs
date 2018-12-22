using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using System.Reactive.Linq;
using Akavache;

namespace Camelotia.Services.Storages
{
    public sealed class AkavacheTokenStorage : ITokenStorage
    {
        public AkavacheTokenStorage() => BlobCache.ApplicationName = "Camelotia";

        public async Task<string> ReadToken<TOwner>()
        {
            var key = typeof(TOwner).Name;
            var token = await BlobCache.Secure.GetOrCreateObject<string>(key, () => null);
            return token;
        }

        public async Task WriteToken<TOwner>(string token)
        {
            var key = typeof(TOwner).Name;
            await BlobCache.Secure.InsertObject(key, token);
        }
    }
}
