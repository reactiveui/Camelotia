using System.ComponentModel;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;

namespace Camelotia.Presentation.Interfaces;

public interface IRenameFileViewModel :
    INotifyPropertyChanged,
    INotifyDataErrorInfo,
    IValidatableViewModel,
    IReactiveObject
{
    bool IsLoading { get; }

    bool IsVisible { get; set; }

    string OldName { get; }

    string NewName { get; set; }

    bool HasErrorMessage { get; }

    string ErrorMessage { get; }

    ReactiveCommand<Unit, Unit> Rename { get; }

    ReactiveCommand<Unit, Unit> Close { get; }

    ReactiveCommand<Unit, Unit> Open { get; }
}