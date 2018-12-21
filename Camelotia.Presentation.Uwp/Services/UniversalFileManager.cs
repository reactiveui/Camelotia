using Camelotia.Services.Interfaces;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalFileManager : IFileManager
    {
        public async Task<(string Name, Stream Stream)> OpenRead()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();
            if (file == null) return (null, null);
            var stream = await file.OpenStreamForReadAsync();
            return (file.Name, stream);
        }

        public async Task<Stream> OpenWrite(string name)
        {
            var ext = Path.GetExtension(name);
            var picker = new FileSavePicker { SuggestedFileName = name };
            picker.FileTypeChoices.Add(ext, new List<string> { ext });
            var file = await picker.PickSaveFileAsync();
            if (file == null) return null;
            await FileIO.WriteTextAsync(file, string.Empty);
            var stream = await file.OpenStreamForWriteAsync();
            return stream;
        }
    }
}
