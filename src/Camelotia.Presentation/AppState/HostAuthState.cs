using System.Runtime.Serialization;

namespace Camelotia.Presentation.AppState;

[DataContract]
public class HostAuthState
{
    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public string Password { get; set; }

    [DataMember]
    public string Address { get; set; }

    [DataMember]
    public string Port { get; set; }
}