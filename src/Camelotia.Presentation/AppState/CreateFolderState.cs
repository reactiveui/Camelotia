using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class CreateFolderState
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
    }
}