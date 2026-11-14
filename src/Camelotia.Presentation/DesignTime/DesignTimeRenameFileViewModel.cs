using System.Reactive;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.Validation.Reactive.Extensions;
using ReactiveUI.Validation.Reactive.Helpers;

namespace Camelotia.Presentation.DesignTime;

public class DesignTimeRenameFileViewModel : ReactiveValidationObject, IRenameFileViewModel
{
    public DesignTimeRenameFileViewModel() => this.ValidationRule(x => x.NewName, name => false, "Validation error.");

    public bool IsLoading { get; }

    public bool IsVisible { get; set; }

    public string OldName { get; } = "file";

    public string NewName { get; set; }

    public bool HasErrorMessage { get; } = true;

    public string ErrorMessage { get; } = "Error message example.";

    public ReactiveCommand<Unit, Unit> Rename { get; }

    public ReactiveCommand<Unit, Unit> Close { get; }

    public ReactiveCommand<Unit, Unit> Open { get; }
}