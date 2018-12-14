using System.IO;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IFileManager
    {
        Task<Stream> OpenWrite(string name);
        
        Task<(string Name, Stream Stream)> OpenRead();
    }
}