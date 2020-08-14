using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using Akavache;

namespace Camelotia.Services.Storages
{
    public sealed class AkavacheStorage : IStorage
    {
        private readonly SourceCache<ProviderModel, Guid> _connectable = new SourceCache<ProviderModel, Guid>(x => x.Id);
        private readonly IDictionary<string, Func<ProviderModel, IProvider>> _factories;
        private readonly IObservable<IChangeSet<IProvider, Guid>> _connection; 
        private readonly IBlobCache _blobCache;

        public AkavacheStorage(IDictionary<string, Func<ProviderModel, IProvider>> factories, IBlobCache blobCache)
        {
            _blobCache = blobCache;
            _factories = factories;
            _connection = _connectable
                .Connect()
                .Publish()
                .RefCount()
                .Transform(model => _factories[model.Type](model));
        }

        public IEnumerable<string> SupportedTypes => _factories.Keys;

        public IObservable<IChangeSet<IProvider, Guid>> Read() => _connection;

        public async Task Add(string typeName)
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

            await _blobCache.InsertObject(guid.ToString(), model);
            _connectable.AddOrUpdate(model);
        }

        public async Task Remove(Guid id)
        {
            var persistentId = id.ToString();
            await _blobCache.InvalidateObject<ProviderModel>(persistentId);
            _connectable.Remove(id);
        }

        public async Task Refresh()
        {
            _connectable.Clear();
            var models = await _blobCache.GetAllObjects<ProviderModel>();
            var relevant = models.Where(model => model != null && _factories.ContainsKey(model.Type));
            _connectable.AddOrUpdate(relevant);
        }
    }
}