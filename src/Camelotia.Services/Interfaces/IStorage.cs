using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;

namespace Camelotia.Services.Interfaces
{
    public interface IStorage
    {
        IEnumerable<string> SupportedTypes { get; }

        IObservable<IChangeSet<IProvider, Guid>> Read();

        Task Add(string type);

        Task Remove(Guid id);
        
        Task Refresh();
    }
}