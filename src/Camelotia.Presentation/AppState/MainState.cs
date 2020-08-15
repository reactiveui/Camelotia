using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using Newtonsoft.Json;

namespace Camelotia.Presentation.AppState
{
    public class MainState
    {
        [JsonIgnore]
        public SourceCache<ProviderState, Guid> Providers { get; } = new SourceCache<ProviderState, Guid>(x => x.Id);

        public IEnumerable<ProviderState> ProviderStates
        {
            get => Providers.Items.ToList();
            set => Providers.AddOrUpdate(value);
        }
    }
}