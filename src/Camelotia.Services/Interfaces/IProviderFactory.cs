using System.Collections.Generic;
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces
{
    public interface IProviderFactory
    {
        IProvider CreateProvider(ProviderModel parameters);
        
        IReadOnlyCollection<ProviderType> SupportedTypes { get; }
    }
}