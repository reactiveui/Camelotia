using Camelotia.Presentation.Interfaces;
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

        public override int GetHashCode() => (Name, Path, IsFolder, Size).GetHashCode();

        public override bool Equals(object instance)
        {
            return instance is FileViewModel file &&
                   file.Name == Name &&
                   file.Path == Path &&
                   file.IsFolder == IsFolder &&
                   file.Size == Size;
        }

        public IProviderViewModel Provider { get; }

        public string Modified => _file.Modified;

        public bool IsFolder => _file.IsFolder;

        public bool IsFile => _file.IsFile;

        public string Name => _file.Name;

        public string Path => _file.Path;

        public string Size => _file.Size;
    }
}
