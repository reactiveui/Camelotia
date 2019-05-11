using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels
{
    public sealed class AuthViewModel : ReactiveObject, IAuthViewModel
    {
        private readonly ObservableAsPropertyHelper<bool> _isAuthenticated;
        private readonly ObservableAsPropertyHelper<bool> _isAnonymous;
        private readonly IProvider _provider;
        
        public AuthViewModel(
            IDirectAuthViewModel directAuth,
            IHostAuthViewModel hostAuth,
            IOAuthViewModel oAuth,
            IProvider provider,
            IScheduler current,
            IScheduler main)
        {
            OAuth = oAuth;
            HostAuth = hostAuth;
            DirectAuth = directAuth;
            _provider = provider;

            _isAuthenticated = _provider
                .IsAuthorized
                .DistinctUntilChanged()
                .ObserveOn(main)
                .Log(this, $"Authentication state changed for {provider.Name}")
                .ToProperty(this, x => x.IsAuthenticated, scheduler: current);

            _isAnonymous = this
                .WhenAnyValue(x => x.IsAuthenticated)
                .Select(authenticated => !authenticated)
                .ToProperty(this, x => x.IsAnonymous, scheduler: current);
        }

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool SupportsHostAuth => _provider.SupportsHostAuth;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        public bool IsAuthenticated => _isAuthenticated.Value;

        public bool IsAnonymous => _isAnonymous.Value;

        public IDirectAuthViewModel DirectAuth { get; }
        
        public IHostAuthViewModel HostAuth { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}