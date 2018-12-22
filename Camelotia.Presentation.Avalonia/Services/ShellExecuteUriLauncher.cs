using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class ShellExecuteUriLauncher : IUriLauncher
    {
        public Task LaunchUri(Uri uri) => Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = uri.ToString()
                }
            };
            process.Start();
        });
    }
}
