// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Globalization;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Runs a provider's host, port, user name and password sign-in form.</summary>
[System.Diagnostics.DebuggerDisplay("HostAuthViewModel: {Address}:{Port}")]
public sealed partial class HostAuthViewModel : ReactiveValidationObject, IHostAuthViewModel
{
    /// <summary>Holds the subscriptions that keep the form and its saved state in step.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Backs <see cref="ErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _errorMessage;

    /// <summary>Backs <see cref="HasErrorMessage"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _hasErrorMessage;

    /// <summary>Backs <see cref="IsBusy"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isBusy;

    /// <summary>Initializes a new instance of the <see cref="HostAuthViewModel"/> class.</summary>
    /// <param name="state">The saved form state.</param>
    /// <param name="provider">The provider to sign in to.</param>
    public HostAuthViewModel(HostAuthState state, ICloud provider)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(provider);

        _ = this.ValidationRule(
            static x => x.Username,
            static name => !string.IsNullOrWhiteSpace(name),
            "User name shouldn't be null or white space.");

        _ = this.ValidationRule(
            static x => x.Password,
            static pass => !string.IsNullOrWhiteSpace(pass),
            "Password shouldn't be null or white space.");

        _ = this.ValidationRule(
            static x => x.Address,
            static host => !string.IsNullOrWhiteSpace(host),
            "Host address shouldn't be null or white space.");

        _ = this.ValidationRule(
            static x => x.Port,
            static port => int.TryParse(port, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "Port should be a valid integer.");

        Login = ReactiveCommand.CreateFromTask(
            cancellationToken => provider.HostAuth(Address, int.Parse(Port, CultureInfo.InvariantCulture), Username, Password, cancellationToken).AsTask(),
            this.IsValid());

        _isBusy = Login.IsExecuting.ToProperty(this, static x => x.IsBusy);

        _errorMessage = Login
            .ThrownExceptions
            .Select(static exception => exception.Message)
            .Log(this, $"Host auth error occured in {provider.Name}")
            .ToProperty(this, static x => x.ErrorMessage, string.Empty);

        _hasErrorMessage = Login
            .ThrownExceptions
            .Select(static _ => true)
            .Merge(Login.Select(static _ => false))
            .ToProperty(this, static x => x.HasErrorMessage);

        Username = state.Username;
        Password = state.Password;
        Address = state.Address;
        Port = state.Port;

        _subscriptions.Add(this.WhenAnyValue(static x => x.Username).Subscribe(name => state.Username = name));
        _subscriptions.Add(this.WhenAnyValue(static x => x.Password).Subscribe(pass => state.Password = pass));
        _subscriptions.Add(this.WhenAnyValue(static x => x.Address).Subscribe(address => state.Address = address));
        _subscriptions.Add(this.WhenAnyValue(static x => x.Port).Subscribe(port => state.Port = port));
    }

    /// <inheritdoc/>
    [Reactive]
    public partial string Port { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial string Address { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial string Username { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial string Password { get; set; }

    /// <inheritdoc/>
    public string ErrorMessage => _errorMessage.Value;

    /// <inheritdoc/>
    public bool HasErrorMessage => _hasErrorMessage.Value;

    /// <inheritdoc/>
    public bool IsBusy => _isBusy.Value;

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Login { get; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subscriptions.Dispose();
            _errorMessage.Dispose();
            _hasErrorMessage.Dispose();
            _isBusy.Dispose();
            Login.Dispose();
        }

        base.Dispose(disposing);
    }
}
