using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services
{
    public sealed class AvaloniaFileManager : IFileManager
    {
        public async Task<Stream> OpenWrite(string name)
        {
            var fileDialog = new OpenFolderDialog();
            var folder = await fileDialog.ShowAsync(Application.Current.MainWindow);
            var path = Path.Combine(folder, name);
            return File.Create(path);
        }

        public async Task<(string Name, Stream Stream)> OpenRead()
        {
            var fileDialog = new OpenFileDialog {AllowMultiple = false};
            var files = await fileDialog.ShowAsync(Application.Current.MainWindow);
            var path = files.First();
            
            var attributes = File.GetAttributes(path);
            var isFolder = attributes.HasFlag(FileAttributes.Directory);
            if (isFolder) throw new Exception("Folders are not supported.");

            var stream = File.OpenRead(path);
            var name = Path.GetFileName(path);
            return (name, stream);
        }
    }
}