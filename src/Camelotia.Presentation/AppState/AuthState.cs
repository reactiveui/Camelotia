using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState;

[DataContract]
public class AuthState
{
    [DataMember]
    public DirectAuthState DirectAuthState { get; set; } = new();

    [DataMember]
    public HostAuthState HostAuthState { get; set; } = new();
}
