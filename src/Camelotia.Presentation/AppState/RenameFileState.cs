using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class RenameFileState
    {
        [DataMember]
        public string NewName { get; set; }
    }
}