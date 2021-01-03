using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Wpf.Services
{
    public sealed class WindowsPresentationFileManager : IFileManager
    {
        public Task<(string Name, Stream Stream)> OpenRead()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                throw new Exception("Nothing selected.");

            var path = dialog.FileName;
            var name = Path.GetFileName(path);
            Stream stream = File.OpenRead(path);
            return Task.FromResult((name, stream));
        }

        public Task<Stream> OpenWrite(string name)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    throw new Exception("Nothing selected.");

                var directory = dialog.SelectedPath;
                var file = Path.Combine(directory, name);
                Stream stream = File.Create(file);
                return Task.FromResult(stream);
            }
        }
    }
}
