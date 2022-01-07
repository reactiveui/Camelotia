using System.Reactive;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.ViewModels;

public sealed class OAuthViewModel : ReactiveObject, IOAuthViewModel
{
    private readonly ObservableAsPropertyHelper<string> _errorMessage;
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
    private readonly ObservableAsPropertyHelper<bool> _isBusy;

    public OAuthViewModel(ICloud provider)
    {
        Login = ReactiveCommand.CreateFromTask(provider.OAuth);

        _isBusy = Login
            .IsExecuting
            .ToProperty(this, x => x.IsBusy);

        _errorMessage = Login
            .ThrownExceptions
            .Select(exception => exception.Message)
            .Log(this, $"OAuth error occured in {provider.Name}")
            .ToProperty(this, x => x.ErrorMessage);

        _hasErrorMessage = Login
            .ThrownExceptions
            .Select(exception => true)
            .Merge(Login.Select(unit => false))
            .ToProperty(this, x => x.HasErrorMessage);
    }

    public ReactiveCommand<Unit, Unit> Login { get; }

    public string ErrorMessage => _errorMessage.Value;

    public bool HasErrorMessage => _hasErrorMessage.Value;

    public bool IsBusy => _isBusy.Value;
}