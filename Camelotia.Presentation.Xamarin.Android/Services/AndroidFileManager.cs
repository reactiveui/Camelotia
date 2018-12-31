using Camelotia.Services.Interfaces;
using Plugin.FilePicker;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Camelotia.Presentation.Xamarin.Android.Services
{
    public sealed class AndroidFileManager : IFileManager
    {
        public async Task<(string Name, Stream Stream)> OpenRead()
        {
            var file = await CrossFilePicker.Current.PickFile();
            if (file == null) return (null, null);

            var fileName = file.FileName;
            var stream = file.GetStream();
            return (fileName, stream);
        }

        public Task<Stream> OpenWrite(string name) => Task.Run(() =>
        {
            var personalPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = Path.Combine(personalPath, name);
            return (Stream)File.Create(filePath);
        });
    }
}
