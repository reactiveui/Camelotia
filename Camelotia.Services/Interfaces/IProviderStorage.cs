using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynamicData;

namespace Camelotia.Services.Interfaces
{
    public interface IProviderStorage
    {
        IEnumerable<string> SupportedTypes { get; }

        IObservable<IChangeSet<IProvider, Guid>> Providers();

        Task Add(string type);

        Task Remove(Guid id);
        
        Task Refresh();
    }
}