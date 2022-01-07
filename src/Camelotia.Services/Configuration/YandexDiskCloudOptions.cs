using System.Runtime.Serialization;

namespace Camelotia.Services.Configuration;

[DataContract]
public class YandexDiskCloudOptions
{
    [DataMember]
    public string HashAuthClientId { get; set; } = "7762e3fccbe3431db2652a8434618790";

    [DataMember]
    public string CodeAuthClientSecret { get; set; } = "317a14f5491447e8bd3a9e7e14ce46cd";

    [DataMember]
    public string CodeAuthClientId { get; set; } = "122661520b174cb5b85b4a3c26aa66f6";
}