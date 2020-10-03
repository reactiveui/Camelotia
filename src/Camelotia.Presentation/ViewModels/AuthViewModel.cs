using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class AuthViewModel : ReactiveObject, IAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _isAuthenticated;
        private readonly ObservableAsPropertyHelper<bool> _isAnonymous;
        private readonly IProvider _provider;
        
        public AuthViewModel(
            IDirectAuthViewModel direct,
            IHostAuthViewModel host,
            IOAuthViewModel open,
            IProvider provider)
        {
            OAuth = open;
            HostAuth = host;
            DirectAuth = direct;
            _provider = provider;
            
            _isAuthenticated = _provider
                .IsAuthorized
                .DistinctUntilChanged()
                .Log(this, $"Authentication state changed for {provider.Name}")
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.IsAuthenticated);

            _isAnonymous = this
                .WhenAnyValue(x => x.IsAuthenticated)
                .Select(authenticated => !authenticated)
                .ToProperty(this, x => x.IsAnonymous);
        }

        public bool IsAnonymous => _isAnonymous.Value;

        public bool IsAuthenticated => _isAuthenticated.Value;

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool SupportsHostAuth => _provider.SupportsHostAuth;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        public IDirectAuthViewModel DirectAuth { get; }
        
        public IHostAuthViewModel HostAuth { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}