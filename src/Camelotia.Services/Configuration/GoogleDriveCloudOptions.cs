using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class GoogleDriveCloudOptions
    {
        [DataMember]
        public string GoogleDriveApplicationName { get; set; } = "Camelotia";

        [DataMember]
        public string GoogleDriveClientId { get; set; } = "1096201018044-qbv35mo5cd7b5utfjpg83v5lsuhssvvg.apps.googleusercontent.com";

        [DataMember]
        public string GoogleDriveClientSecret { get; set; } = "L-xoeULle07kb_jHleqMxWo2";

        [DataMember]
        public string GoogleDriveUserName { get; set; } = "user";
    }
}
