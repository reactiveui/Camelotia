using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Camelotia.Services.Configuration;
using Camelotia.Services.Models;
using DynamicData;

namespace Camelotia.Presentation.AppState;

[DataContract]
public class MainState
{
    [IgnoreDataMember]
    [JsonIgnore]
    public SourceCache<CloudState, Guid> Clouds { get; } = new(x => x.Id);

    [DataMember]
    public IEnumerable<CloudState> CloudStates
    {
        get => new List<CloudState>(Clouds.Items);
        set => Clouds.AddOrUpdate(value);
    }

    [DataMember]
    public CloudType? SelectedSupportedType { get; set; }

    [DataMember]
    public Guid SelectedProviderId { get; set; }

    [DataMember]
    public CloudConfiguration CloudConfiguration { get; set; } = new();
}
