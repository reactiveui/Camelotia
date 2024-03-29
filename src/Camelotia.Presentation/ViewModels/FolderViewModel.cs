﻿using System.Collections.Generic;
using System.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels;

public delegate IFolderViewModel FolderViewModelFactory(FolderModel folder, ICloudViewModel provider);

public sealed class FolderViewModel : ReactiveObject, IFolderViewModel
{
    private readonly FolderModel _folder;

    public FolderViewModel(ICloudViewModel provider, FolderModel folder)
    {
        Provider = provider;
        _folder = folder;
        Children = folder.Children?.Any() == true
            ? folder.Children.Select(f => new FolderViewModel(provider, f))
            : Enumerable.Empty<IFolderViewModel>();
    }

    public ICloudViewModel Provider { get; }

    public string Name => _folder.Name;

    public string FullPath => _folder.FullPath;

    public IEnumerable<IFolderViewModel> Children { get; }

    public override bool Equals(object obj)
    {
        return obj is FolderViewModel other &&
               Equals(_folder.Name, other._folder.Name) &&
               Equals(_folder.FullPath, other._folder.FullPath) &&
               Equals(Provider, other.Provider);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _folder.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ _folder.FullPath.GetHashCode();
            hashCode = (hashCode * 397) ^ Provider.GetHashCode();
            return hashCode;
        }
    }
}