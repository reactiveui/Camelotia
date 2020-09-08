using Camelotia.Presentation.Interfaces;
using System.Collections.Generic;

namespace Camelotia.Presentation.ViewModels
{
    public class FolderViewModel : IFolderViewModel
    {
        public string Name { get; set; }
        public IEnumerable<string> Children { get; set; }
    }
}
