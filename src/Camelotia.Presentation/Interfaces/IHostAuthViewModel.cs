using ReactiveUI;
using ReactiveUI.Validation.Abstractions;

namespace Camelotia.Presentation.Interfaces
{
    public interface IHostAuthViewModel :
        IDirectAuthViewModel,
        IValidatableViewModel,
        IReactiveObject
    {
        string Address { get; set; }
        
        string Port { get; set; }
    }
}