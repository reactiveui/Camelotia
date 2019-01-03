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
            IScheduler currentThread,
            IScheduler mainThread,
            IProvider provider)
        {
            OAuth = oAuth;
            HostAuth = hostAuth;
            DirectAuth = directAuth;
            _provider = provider;

            _isAuthenticated = _provider
                .IsAuthorized
                .DistinctUntilChanged()
                .ObserveOn(mainThread)
                .ToProperty(this, x => x.IsAuthenticated, scheduler: currentThread);

            _isAnonymous = _provider
                .IsAuthorized
                .Select(authorized => !authorized)
                .DistinctUntilChanged()
                .ObserveOn(mainThread)
                .ToProperty(this, x => x.IsAnonymous, scheduler: currentThread);
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