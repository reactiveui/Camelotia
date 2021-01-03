using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class CreateFolderState
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsVisible { get; set; }
    }
}