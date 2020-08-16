using System.Collections.Generic;
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces
{
    public interface IProviderFactory
    {
        IProvider CreateProvider(ProviderParameters parameters);
        
        IReadOnlyCollection<ProviderType> SupportedTypes { get; }
    }
}