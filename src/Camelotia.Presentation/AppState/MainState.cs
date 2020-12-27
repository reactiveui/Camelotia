using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Camelotia.Services.Models;
using DynamicData;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class MainState
    {
        [IgnoreDataMember]
        public SourceCache<CloudState, Guid> Clouds { get; } = new SourceCache<CloudState, Guid>(x => x.Id);

        [DataMember]
        public IEnumerable<CloudState> CloudStates
        {
            get => Clouds.Items.ToList();
            set => Clouds.AddOrUpdate(value);
        }
        
        [DataMember]
        public CloudType? SelectedSupportedType { get; set; }
        
        [DataMember]
        public Guid SelectedProviderId { get; set; }
    }
}