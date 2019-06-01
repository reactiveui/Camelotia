using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using Akavache;

namespace Camelotia.Services.Storages
{
    public sealed class ProviderStorage : IProviderStorage
    {
        private readonly SourceCache<IProvider, Guid> _connectable = new SourceCache<IProvider, Guid>(x => x.Id);
        private readonly IDictionary<string, Func<ProviderModel, IProvider>> _factories;
        private readonly IBlobCache _blobCache;

        public ProviderStorage(
            IDictionary<string, Func<ProviderModel, IProvider>> factories,
            IBlobCache blobCache)
        {
            _blobCache = blobCache;
            _factories = factories;
        }

        public IEnumerable<string> SupportedTypes => _factories.Keys;

        public IObservable<IChangeSet<IProvider, Guid>> Read() => _connectable.Connect();

        public Task Add(string typeName) => Task.Run(() =>
        {
            var guid = Guid.NewGuid();
            var type = _factories.Keys.First(x => x == typeName);
            var model = new ProviderModel
            {
                Id = guid,
                Type = type,
                Token = null,
                Created = DateTime.Now
            };

            _blobCache.InsertObject(guid.ToString(), model).Subscribe();
            var provider = _factories[type](model);
            _connectable.AddOrUpdate(provider);
        });

        public Task Remove(Guid id) => Task.Run(() =>
        {
            var persistentId = id.ToString();
            _blobCache.InvalidateObject<ProviderModel>(persistentId).Subscribe();
            var provider = _connectable.Items.First(x => x.Id == id);
            _connectable.Remove(provider);
        });

        public async Task Refresh()
        {
            _connectable.Clear();
            var models = await _blobCache
                .GetAllObjects<ProviderModel>()
                .SubscribeOn(Scheduler.Default);

            var providers = models
                .Where(model => model != null && _factories.ContainsKey(model.Type))
                .Select(model => _factories[model.Type](model));

            _connectable.Edit(cache =>
            {
                foreach (var provider in providers)
                    cache.AddOrUpdate(provider);
            });
        }
    }
}