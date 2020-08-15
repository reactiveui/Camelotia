using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class RenameFileState
    {
        public string NewName { get; set; }
        public bool IsVisible { get; set; }
    }
}