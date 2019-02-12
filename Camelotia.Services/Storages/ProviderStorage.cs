using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using DynamicData;

namespace Camelotia.Services.Storages
{
    public sealed class ProviderStorage : IProviderStorage
    {
        private readonly SourceList<IProvider> _connectable = new SourceList<IProvider>();
        private readonly IEnumerable<IProvider> _providers;

        public ProviderStorage(params IProvider[] providers) => _providers = providers;

        public IObservable<IChangeSet<IProvider>> Connect() => _connectable.Connect();
        
        public async Task LoadProviders()
        {
            _connectable.Clear();
            await Task.Delay(1000);
            _connectable.AddRange(_providers);
        }
    }
}