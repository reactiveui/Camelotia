using System.Net;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IListener
    {    
        void Start(IPAddress address, int port);
        
        Task<string> GetCode();

        void Stop();
    }
}