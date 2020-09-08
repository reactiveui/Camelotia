using System.Collections.Generic;

namespace Camelotia.Presentation.Interfaces
{
    public interface IFolderViewModel
    {
        public string Name { get; }

        public IEnumerable<string> Children { get; }
    }
}
