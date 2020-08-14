using System;
using DynamicData;

namespace Camelotia.Presentation.AppState
{
    public class MainState
    {
        public SourceCache<ProviderState, Guid> Providers { get; } = new SourceCache<ProviderState, Guid>(x => x.Id);
    }
}