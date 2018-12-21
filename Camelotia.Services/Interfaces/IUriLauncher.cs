using System;
using System.Threading.Tasks;

namespace Camelotia.Services.Interfaces
{
    public interface IUriLauncher
    {
        Task LaunchUri(Uri uri);
    }
}
