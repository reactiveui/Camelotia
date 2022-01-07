using System.Collections.Generic;
using System.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime;

public class DesignTimeFolderViewModel : ReactiveObject, IFolderViewModel
{
    public DesignTimeFolderViewModel()
    {
        Name = "home";
        Children = new List<IFolderViewModel>
        {
            new DesignTimeFolderViewModel("home"),
            new DesignTimeFolderViewModel("home1"),
            new DesignTimeFolderViewModel("home2")
        };
    }

    public DesignTimeFolderViewModel(string name, IEnumerable<IFolderViewModel> children = null)
    {
        Name = name;
        Children = children ?? Enumerable.Empty<IFolderViewModel>();
    }

    public string Name { get; }

    public string FullPath { get; }

    public IEnumerable<IFolderViewModel> Children { get; }
}