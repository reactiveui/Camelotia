using System.Reactive;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.DesignTime;

public class DesignTimeHostAuthViewModel : ReactiveValidationObject, IHostAuthViewModel
{
    public DesignTimeHostAuthViewModel() => this.ValidationRule(x => x.Username, name => false, "Validation error.");

    public string Username { get; set; } = "Jotaro";

    public string Password { get; set; } = "Qwerty";

    public ReactiveCommand<Unit, Unit> Login { get; }

    public bool HasErrorMessage { get; } = true;

    public string ErrorMessage { get; } = "Error message example.";

    public bool IsBusy { get; }

    public string Address { get; set; } = "127.0.0.1";

    public string Port { get; set; } = "5001";
}