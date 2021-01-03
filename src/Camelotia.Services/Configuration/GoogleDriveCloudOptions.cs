using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class GoogleDriveCloudOptions
    {
        [DataMember]
        public string GoogleDriveApplicationName { get; set; }

        [DataMember]
        public string GoogleDriveClientId { get; set; }

        [DataMember]
        public string GoogleDriveClientSecret { get; set; }

        [DataMember]
        public string GoogleDriveUserName { get; set; }
    }
}
