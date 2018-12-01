using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class AuthViewModel : ReactiveObject, IAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _isAuthenticated;
        private readonly IProvider _provider;
        
        public AuthViewModel(
            IDirectAuthViewModel directAuth,
            IOAuthViewModel oAuth,
            IProvider provider)
        {
            var main = RxApp.MainThreadScheduler;
            Activator = new ViewModelActivator();
            DirectAuth = directAuth;
            OAuth = oAuth;
            
            _provider = provider;
            _isAuthenticated = _provider.IsAuthorized
                .DistinctUntilChanged()
                .ToProperty(this, x => x.IsAuthenticated, scheduler: main);
        }
        
        public bool IsAuthenticated => _isAuthenticated.Value;

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        public IDirectAuthViewModel DirectAuth { get; }
        
        public ViewModelActivator Activator { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}