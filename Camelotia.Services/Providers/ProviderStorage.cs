using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Services.Providers
{
    public sealed class ProviderStorage : IProviderStorage
    {
        private readonly IEnumerable<IProvider> _providers;

        public ProviderStorage(params IProvider[] providers) => _providers = providers;

        public Task<IEnumerable<IProvider>> LoadProviders() => Task.Run(() =>
        {
            if (_providers.Any()) return _providers;
            var provider = typeof(IProvider);
            var providers = provider.Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && 
                               !type.IsInterface &&
                               provider.IsAssignableFrom(type))
                .Select(Activator.CreateInstance)
                .Cast<IProvider>();
            return providers;
        });
    }
}