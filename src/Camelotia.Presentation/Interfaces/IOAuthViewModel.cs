using System.ComponentModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Interfaces;

public interface IOAuthViewModel : INotifyPropertyChanged
{
    ReactiveCommand<Unit, Unit> Login { get; }

    bool HasErrorMessage { get; }

    string ErrorMessage { get; }

    bool IsBusy { get; }
}