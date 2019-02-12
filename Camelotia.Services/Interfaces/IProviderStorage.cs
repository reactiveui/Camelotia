using System;
using System.Threading.Tasks;
using DynamicData;

namespace Camelotia.Services.Interfaces
{
    public interface IProviderStorage
    {
        IObservable<IChangeSet<IProvider>> Connect();
        
        Task LoadProviders();
    }
}