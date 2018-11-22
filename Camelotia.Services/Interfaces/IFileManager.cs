using System.IO;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IFileManager
    {
        Task<Stream> OpenWrite();
        
        Task<Stream> OpenRead();
    }
}