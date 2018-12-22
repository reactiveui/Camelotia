using System.Collections.Generic;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Services.Providers
{
    public sealed class ProviderStorage : IProviderStorage
    {
        private readonly IEnumerable<IProvider> _providers;

        public ProviderStorage(params IProvider[] providers) => _providers = providers;

        public Task<IEnumerable<IProvider>> LoadProviders() => Task.FromResult(_providers);
    }
}