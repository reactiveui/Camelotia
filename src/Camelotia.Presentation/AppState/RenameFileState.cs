using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    public class RenameFileState
    {
        public string NewName { get; set; }
        public bool IsVisible { get; set; }
    }
}