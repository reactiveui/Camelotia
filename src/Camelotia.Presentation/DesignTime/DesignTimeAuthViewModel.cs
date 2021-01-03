using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeAuthViewModel : ReactiveObject, IAuthViewModel
    {
        public IDirectAuthViewModel DirectAuth { get; } = new DesignTimeDirectAuthViewModel();

        public IHostAuthViewModel HostAuth { get; } = new DesignTimeHostAuthViewModel();

        public IOAuthViewModel OAuth { get; } = new DesignTimeOAuthViewModel();

        public bool SupportsDirectAuth { get; }

        public bool SupportsHostAuth { get; }

        public bool SupportsOAuth { get; }

        public bool IsAuthenticated { get; } = true;

        public bool IsAnonymous { get; }
    }
}