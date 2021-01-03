using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration
{
    [DataContract]
    public class GitHubCloudOptions
    {
        [DataMember]
        public string GithubApplicationId { get; set; }
    }
}
