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
            IOAuthViewModel oAuth,
            IScheduler currentThread,
            IProvider provider)
        {
            OAuth = oAuth;
            DirectAuth = directAuth;
            _provider = provider;

            _isAuthenticated = _provider
                .IsAuthorized
                .DistinctUntilChanged()
                .ToProperty(this, x => x.IsAuthenticated, scheduler: currentThread);

            _isAnonymous = _provider
                .IsAuthorized
                .Select(authorized => !authorized)
                .DistinctUntilChanged()
                .StartWith(false)
                .ToProperty(this, x => x.IsAnonymous, scheduler: currentThread);
        }

        public bool SupportsDirectAuth => _provider.SupportsDirectAuth;

        public bool IsAuthenticated => _isAuthenticated.Value;

        public bool SupportsOAuth => _provider.SupportsOAuth;

        public bool IsAnonymous => _isAnonymous.Value;

        public IDirectAuthViewModel DirectAuth { get; }
        
        public IOAuthViewModel OAuth { get; }
    }
}