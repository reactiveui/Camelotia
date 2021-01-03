using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class CloudConfiguration
    {
        [DataMember]
        public GitHubCloudOptions GitHub { get; set; } = new GitHubCloudOptions();

        [DataMember]
        public GoogleDriveCloudOptions GoogleDrive { get; set; } = new GoogleDriveCloudOptions();

        [DataMember]
        public VkDocsCloudOptions VkDocs { get; set; } = new VkDocsCloudOptions();

        [DataMember]
        public YandexDiskCloudOptions YandexDisk { get; set; } = new YandexDiskCloudOptions();
    }
}
