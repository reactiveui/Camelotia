using System.Globalization;
using Camelotia.Presentation.Extensions;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IFileViewModel FileViewModelFactory(FileModel file, ICloudViewModel provider);

    public sealed class FileViewModel : ReactiveObject, IFileViewModel
    {
        private readonly FileModel _file;

        public FileViewModel(ICloudViewModel provider, FileModel file)
        {
            Provider = provider;
            _file = file;
        }

        public ICloudViewModel Provider { get; }

        public string Modified => _file.Modified?.ToString(CultureInfo.InvariantCulture) ?? "A long time ago";

        public string Size => _file.Size == 0 ? "Unknown" : _file.Size.ByteSizeToString();

        public bool IsFolder => _file.IsFolder;

        public bool IsFile => !_file.IsFolder;

        public string Name => _file.Name;

        public string Path => _file.Path;

        public override bool Equals(object obj)
        {
            if (obj is FileViewModel other)
                return Equals(_file.Path, other._file.Path) &&
                       Equals(_file.Modified, other._file.Modified) &&
                       Equals(Provider, other.Provider);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _file.Path.GetHashCode();
                hashCode = (hashCode * 397) ^ _file.Modified.GetHashCode();
                hashCode = (hashCode * 397) ^ Provider.GetHashCode();
                return hashCode;
            }
        }
    }
}
