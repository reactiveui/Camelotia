using Camelotia.Services.Interfaces;
using Windows.System;
using System.Threading.Tasks;
using System;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalUriLauncher : IUriLauncher
    {
        public async Task LaunchUri(Uri uri)
        {
            var success = await Launcher.LaunchUriAsync(uri);
            if (!success) throw new Exception("Unable to launch Uri.");
        }
    }
}
