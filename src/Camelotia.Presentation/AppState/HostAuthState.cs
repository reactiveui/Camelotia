using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class HostAuthState
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}