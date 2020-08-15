using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class AuthState
    {
        public DirectAuthState DirectAuthState { get; set; } = new DirectAuthState();
        public HostAuthState HostAuthState { get; set; } = new HostAuthState();
    }
}