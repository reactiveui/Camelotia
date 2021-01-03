using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class VkDocsCloudOptions
    {
        [DataMember]
        public ulong ApplicationId { get; set; }
    }
}
