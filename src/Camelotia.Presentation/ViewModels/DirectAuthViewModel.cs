using System;
using System.Reactive;
using System.Reactive.Linq;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels;

public sealed class DirectAuthViewModel : ReactiveValidationObject, IDirectAuthViewModel
{
    private readonly ObservableAsPropertyHelper<string> _errorMessage;
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
    private readonly ObservableAsPropertyHelper<bool> _isBusy;

    public DirectAuthViewModel(DirectAuthState state, ICloud provider)
    {
        this.ValidationRule(
            x => x.Username,
            name => !string.IsNullOrWhiteSpace(name),
            "User name shouldn't be null or white space.");

        this.ValidationRule(
            x => x.Password,
            pass => !string.IsNullOrWhiteSpace(pass),
            "Password shouldn't be null or white space.");

        Login = ReactiveCommand.CreateFromTask(
            () => provider.DirectAuth(Username, Password),
            this.IsValid());

        _isBusy = Login
            .IsExecuting
            .ToProperty(this, x => x.IsBusy);

        _errorMessage = Login
            .ThrownExceptions
            .Select(exception => exception.Message)
            .Log(this, $"Direct auth error occured in {provider.Name}")
            .ToProperty(this, x => x.ErrorMessage);

        _hasErrorMessage = Login
            .ThrownExceptions
            .Select(exception => true)
            .Merge(Login.Select(unit => false))
            .ToProperty(this, x => x.HasErrorMessage);

        Username = state.Username;
        Password = state.Password;

        this.WhenAnyValue(x => x.Username)
            .Subscribe(name => state.Username = name);
        this.WhenAnyValue(x => x.Password)
            .Subscribe(pass => state.Password = pass);
    }

    [Reactive]
    public string Username { get; set; }

    [Reactive]
    public string Password { get; set; }

    public bool IsBusy => _isBusy.Value;

    public string ErrorMessage => _errorMessage.Value;

    public bool HasErrorMessage => _hasErrorMessage.Value;

    public ReactiveCommand<Unit, Unit> Login { get; }
}