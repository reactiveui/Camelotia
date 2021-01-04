using System.Reactive;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeOAuthViewModel : ReactiveObject, IOAuthViewModel
    {
        public ReactiveCommand<Unit, Unit> Login { get; }

        public bool HasErrorMessage { get; } = true;

        public string ErrorMessage { get; } = "Error message example.";

        public bool IsBusy { get; }
    }
}