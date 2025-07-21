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

public sealed class HostAuthViewModel : ReactiveValidationObject, IHostAuthViewModel
{
    private readonly ObservableAsPropertyHelper<string> _errorMessage;
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;
    private readonly ObservableAsPropertyHelper<bool> _isBusy;

    public HostAuthViewModel(HostAuthState state, ICloud provider)
    {
        this.ValidationRule(
            x => x.Username,
            name => !string.IsNullOrWhiteSpace(name),
            "User name shouldn't be null or white space.");

        this.ValidationRule(
            x => x.Password,
            pass => !string.IsNullOrWhiteSpace(pass),
            "Password shouldn't be null or white space.");

        this.ValidationRule(
            x => x.Address,
            host => !string.IsNullOrWhiteSpace(host),
            "Host address shouldn't be null or white space.");

        this.ValidationRule(
            x => x.Port,
            port => int.TryParse(port, out _),
            "Port should be a valid integer.");

        Login = ReactiveCommand.CreateFromTask(
            () => provider.HostAuth(Address, int.Parse(Port), Username, Password),
            this.IsValid());

        _isBusy = Login
            .IsExecuting
            .ToProperty(this, x => x.IsBusy);

        _errorMessage = Login
            .ThrownExceptions
            .Select(exception => exception.Message)
            .Log(this, $"Host auth error occured in {provider.Name}")
            .ToProperty(this, x => x.ErrorMessage);

        _hasErrorMessage = Login
            .ThrownExceptions
            .Select(exception => true)
            .Merge(Login.Select(unit => false))
            .ToProperty(this, x => x.HasErrorMessage);

        Username = state.Username;
        Password = state.Password;
        Address = state.Address;
        Port = state.Port;

        this.WhenAnyValue(x => x.Username)
            .Subscribe(name => state.Username = name);
        this.WhenAnyValue(x => x.Password)
            .Subscribe(name => state.Password = name);
        this.WhenAnyValue(x => x.Address)
            .Subscribe(name => state.Address = name);
        this.WhenAnyValue(x => x.Port)
            .Subscribe(name => state.Port = name);
    }

    [Reactive]
    public string Port { get; set; }

    [Reactive]
    public string Address { get; set; }

    [Reactive]
    public string Username { get; set; }

    [Reactive]
    public string Password { get; set; }

    public string ErrorMessage => _errorMessage.Value;

    public bool HasErrorMessage => _hasErrorMessage.Value;

    public bool IsBusy => _isBusy.Value;

    public ReactiveCommand<Unit, Unit> Login { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _errorMessage?.Dispose();
            _hasErrorMessage?.Dispose();
            _isBusy?.Dispose();
            Login?.Dispose();
        }

        base.Dispose(disposing);
    }
}
