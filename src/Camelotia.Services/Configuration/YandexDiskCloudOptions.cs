using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class YandexDiskCloudOptions
    {
        [DataMember]
        public string HashAuthClientId { get; }

        [DataMember]
        public string CodeAuthClientSecret { get; }

        [DataMember]
        public string CodeAuthClientId { get; }
    }
}
