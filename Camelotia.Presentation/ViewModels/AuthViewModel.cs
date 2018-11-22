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
            _provider = provider;
            DirectAuth = directAuth;
            OAuth = oAuth;
            
            Activator = new ViewModelActivator();
            _isAuthenticated = _provider.IsAuthorized
                .DistinctUntilChanged()
                .ToProperty(this, x => x.IsAuthenticated);
        }
        
        public bool IsAuthenticated => _isAuthenticated.Value;

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        public IDirectAuthViewModel DirectAuth { get; }
        
        public ViewModelActivator Activator { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}