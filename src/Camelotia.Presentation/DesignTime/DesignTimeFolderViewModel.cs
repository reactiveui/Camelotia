using Camelotia.Presentation.Interfaces;
using System.Collections.Generic;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeFolderViewModel : IFolderViewModel
    {
        public string Name { get; set; }

        public string FullPath { get; set; }

        public IEnumerable<IFolderViewModel> Children { get; set; }
    }
}
