using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Camelotia.Services.Models;
using DynamicData;
using Newtonsoft.Json;

namespace Camelotia.Presentation.AppState
{
    [DataContract]
    public class MainState
    {
        [IgnoreDataMember]
        public SourceCache<ProviderState, Guid> Providers { get; } = new SourceCache<ProviderState, Guid>(x => x.Id);

        [DataMember]
        public IEnumerable<ProviderState> ProviderStates
        {
            get => Providers.Items.ToList();
            set => Providers.AddOrUpdate(value);
        }
        
        [DataMember]
        public ProviderType? SelectedSupportedType { get; set; }
        
        [DataMember]
        public Guid SelectedProviderId { get; set; }
    }
}