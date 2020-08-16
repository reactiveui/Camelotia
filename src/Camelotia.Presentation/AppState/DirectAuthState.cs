using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class DirectAuthState
    {
        [DataMember]
        public string Username { get; set; }
        
        [DataMember]
        public string Password { get; set; }
    }
}