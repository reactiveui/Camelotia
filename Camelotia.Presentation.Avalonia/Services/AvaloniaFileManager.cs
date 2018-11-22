using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class AvaloniaFileManager : IFileManager
    {
        public async Task<Stream> OpenWrite()
        {
            var fileDialog = new OpenFolderDialog();
            var folder = await fileDialog.ShowAsync();
            var path = Path.Combine(folder, DateTime.Now.Ticks.ToString());
            return File.Create(path);
        }

        public async Task<Stream> OpenRead()
        {
            var fileDialog = new OpenFileDialog {AllowMultiple = false};
            var files = await fileDialog.ShowAsync();
            var file = files.First();
            
            var attributes = File.GetAttributes(file);
            var isFolder = attributes.HasFlag(FileAttributes.Directory);
            if (isFolder) throw new Exception("Folders are not supported.");

            return File.OpenRead(file);
        }
    }
}