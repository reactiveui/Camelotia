using System.Globalization;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Extensions;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IFileViewModel FileViewModelFactory(FileModel file, IProviderViewModel provider);

    public sealed class FileViewModel : ReactiveObject, IFileViewModel
    {
        private readonly FileModel _file;

        public FileViewModel(IProviderViewModel provider, FileModel file)
        {
            Provider = provider;
            _file = file;
        }

        public IProviderViewModel Provider { get; }

        public string Modified => _file.Modified?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        public string Size => _file.Size == 0 ? string.Empty : _file.Size.ByteSizeToString();

        public bool IsFolder => _file.IsFolder;

        public bool IsFile => !_file.IsFolder;

        public string Name => _file.Name;

        public string Path => _file.Path;
    }
}
