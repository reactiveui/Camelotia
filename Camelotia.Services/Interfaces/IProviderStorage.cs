using System.Collections.Generic;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IProviderStorage
    {
        Task<IEnumerable<IProvider>> LoadProviders();
    }
}