using ReactiveUI;

namespace Camelotia.Presentation.Interfaces
{
    public interface IAuthViewModel : IReactiveObject, ISupportsActivation
    {
        IDirectAuthViewModel DirectAuth { get; }
        
        IOAuthViewModel OAuth { get; }
        
        bool SupportsDirectAuth { get; }
        
        bool SupportsOAuth { get; }
        
        bool IsAuthenticated { get; }
    }
}