using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace Camelotia.Presentation.ViewModels
{
    public delegate IFolderViewModel FolderViewModelFactory(FolderModel folder, IProviderViewModel provider);

    public sealed class FolderViewModel : ReactiveObject, IFolderViewModel
    {
        private readonly FolderModel _folder;

        public FolderViewModel(IProviderViewModel provider, FolderModel folder)
        {
            Provider = provider;
            _folder = folder;
            Children = (folder.Children != null && folder.Children.Any())
                ? folder.Children.Select(f => new FolderViewModel(provider, f))
                : Enumerable.Empty<IFolderViewModel>();
        }

        public IProviderViewModel Provider { get; }

        public string Name => _folder.Name;

        public string FullPath => _folder.FullPath;

        public IEnumerable<IFolderViewModel> Children { get; }

        public override bool Equals(object obj)
        {
            return (obj is FolderViewModel other) &&
                Equals(_folder.Name, other._folder.Name) &&
                Equals(_folder.FullPath, other._folder.FullPath) &&
                Equals(Provider, other.Provider);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _folder.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ _folder.FullPath.GetHashCode();
                hashCode = (hashCode * 397) ^ Provider.GetHashCode();
                return hashCode;
            }
        }
    }
}
