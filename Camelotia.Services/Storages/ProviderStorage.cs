using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;

namespace Camelotia.Services.Storages
{
    public sealed class ProviderStorage : IProviderStorage
    {
        private readonly SourceCache<IProvider, Guid> _connectable = new SourceCache<IProvider, Guid>(x => x.Id);
        private readonly IDictionary<string, Func<Guid, IProvider>> _factories;
        private readonly IBlobCache _blobCache;

        public ProviderStorage(
            IDictionary<string, Func<Guid, IProvider>> factories,
            IBlobCache blobCache)
        {
            _blobCache = blobCache;
            _factories = factories;
        }

        public IEnumerable<string> SupportedTypes => _factories.Keys;

        public IObservable<IChangeSet<IProvider, Guid>> Providers() => _connectable.Connect();

        public Task Add(string typeName) => Task.Run(async () =>
        {
            var type = _factories.Keys.First(x => x == typeName);
            var providerFactory = _factories[type];
            
            var guid = Guid.NewGuid();
            var provider = providerFactory(guid);
            
            var persistentId = guid.ToString();
            await _blobCache.InsertObject(persistentId, new ProviderModel
            {
                Id = guid,
                Type = type,
                Token = null
            });
            
            _connectable.AddOrUpdate(provider);
        });

        public Task Remove(Guid id) => Task.Run(async () =>
        {
            var persistentId = id.ToString();
            await _blobCache.InvalidateObject<ProviderModel>(persistentId);
            
            var provider = _connectable.Items.First(x => x.Id == id);
            _connectable.Remove(provider);
        });

        public Task Refresh() => Task.Run(async () =>
        {
            _connectable.Clear();
            var models = await _blobCache.GetAllObjects<ProviderModel>();
            var providers = models
                .Where(model => model != null && _factories.ContainsKey(model.Type))
                .Select(model => _factories[model.Type](model.Id));

            foreach (var provider in providers) 
                _connectable.AddOrUpdate(provider);
        });
    }
}