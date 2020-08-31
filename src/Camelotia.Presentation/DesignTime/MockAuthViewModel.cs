using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class MockAuthViewModel : ReactiveObject, IAuthViewModel
    {
        public IDirectAuthViewModel DirectAuth { get; } = new MockDirectAuthViewModel();
        
        public IHostAuthViewModel HostAuth { get; } = new MockHostAuthViewModel();
        
        public IOAuthViewModel OAuth { get; } = new MockOAuthViewModel();
        
        public bool SupportsDirectAuth { get; }
        
        public bool SupportsHostAuth { get; }
        
        public bool SupportsOAuth { get; }
        
        public bool IsAuthenticated { get; }
        
        public bool IsAnonymous { get; }
    }
}