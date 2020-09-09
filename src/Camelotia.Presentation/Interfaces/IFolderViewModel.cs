using System.Collections.Generic;

namespace Camelotia.Presentation.Interfaces
{
    public interface IFolderViewModel
    {
        string Name { get; }

        string FullPath { get; }

        IEnumerable<IFolderViewModel> Children { get; }
    }
}
