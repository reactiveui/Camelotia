// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests the sign-in view models.</summary>
[System.Diagnostics.DebuggerDisplay("AuthViewModelTests")]
public sealed class AuthViewModelTests : IDisposable
{
    /// <summary>The user name the forms type.</summary>
    private const string User = "user";

    /// <summary>The server the host form signs in to.</summary>
    private const string Host = "host.example";

    /// <summary>The port the host form signs in to.</summary>
    private const string Port = "2121";

    /// <summary>The provider the forms sign in to.</summary>
    private readonly FakeCloud _cloud = new();

    /// <summary>Follows the provider's sign-in state and capabilities.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task AuthViewModel_FollowsTheProvider()
    {
        _cloud.SupportsDirectAuth = true;
        _cloud.SupportsHostAuth = true;
        _cloud.SupportsOAuth = true;
        var direct = new DirectAuthViewModel(new(), _cloud);
        var host = new HostAuthViewModel(new(), _cloud);
        var oauth = new OAuthViewModel(_cloud);
        using var model = new AuthViewModel(direct, host, oauth, _cloud);
        var signedOut = (model.IsAuthenticated, model.IsAnonymous);

        _cloud.SetAuthorized(true);

        await Assert.That(signedOut).IsEqualTo((false, true));
        await Assert.That(model.IsAuthenticated).IsTrue();
        await Assert.That(model.IsAnonymous).IsFalse();
        await Assert.That((model.SupportsDirectAuth, model.SupportsHostAuth, model.SupportsOAuth)).IsEqualTo((true, true, true));
        await Assert.That(model.DirectAuth).IsSameReferenceAs(direct);
        await Assert.That(model.HostAuth).IsSameReferenceAs(host);
        await Assert.That(model.OAuth).IsSameReferenceAs(oauth);
        _ = await Assert.That(() => new AuthViewModel(direct, host, oauth, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Signs in with a user name and password once both are typed, and saves them.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DirectAuth_SignsInWithValidInput()
    {
        var state = new DirectAuthState();
        using var model = new DirectAuthViewModel(state, _cloud);
        var canLoginEmpty = model.Login.CanRunNow();

        model.Username = User;
        model.Password = TestValues.Password;
        await model.Login.RunAsync(RxVoid.Default);

        await Assert.That(canLoginEmpty).IsFalse();
        await Assert.That(_cloud.Calls).Contains($"DirectAuth {User} {TestValues.Password}");
        await Assert.That(state.Username).IsEqualTo(User);
        await Assert.That(model.HasErrorMessage).IsFalse();
        await Assert.That(model.IsBusy).IsFalse();
        _ = await Assert.That(() => new DirectAuthViewModel(null!, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new DirectAuthViewModel(state, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Reports a failed direct sign-in.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task DirectAuth_ReportsAFailure()
    {
        _cloud.Operation = static _ => Task.FromException(new InvalidOperationException("denied"));
        using var model = new DirectAuthViewModel(new DirectAuthState { Username = User, Password = TestValues.Password }, _cloud);

        await model.Login.RunAsync(RxVoid.Default);

        await Assert.That(model.HasErrorMessage).IsTrue();
        await Assert.That(model.ErrorMessage).IsEqualTo("denied");
    }

    /// <summary>Signs in to a host once every field is valid, and saves the form.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_SignsInWithValidInput()
    {
        var state = new HostAuthState();
        using var model = new HostAuthViewModel(state, _cloud);

        model.Address = Host;
        model.Username = User;
        model.Password = TestValues.Password;
        model.Port = "not a port";
        var canLoginBadPort = model.Login.CanRunNow();
        model.Port = Port;
        await model.Login.RunAsync(RxVoid.Default);

        await Assert.That(canLoginBadPort).IsFalse();
        await Assert.That(_cloud.Calls).Contains($"HostAuth {Host} {Port} {User} {TestValues.Password}");
        await Assert.That((state.Address, state.Port, state.Username)).IsEqualTo((Host, Port, User));
        await Assert.That(model.IsBusy).IsFalse();
        _ = await Assert.That(() => new HostAuthViewModel(null!, _cloud)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new HostAuthViewModel(state, null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Reports a failed host sign-in.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task HostAuth_ReportsAFailure()
    {
        _cloud.Operation = static _ => Task.FromException(new IOException("refused"));
        var state = new HostAuthState { Address = Host, Port = Port, Username = User, Password = TestValues.Password };
        using var model = new HostAuthViewModel(state, _cloud);

        await model.Login.RunAsync(RxVoid.Default);

        await Assert.That(model.HasErrorMessage).IsTrue();
        await Assert.That(model.ErrorMessage).IsEqualTo("refused");
    }

    /// <summary>Signs in through the browser and reports a failure.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task OAuth_SignsInAndReportsAFailure()
    {
        using var model = new OAuthViewModel(_cloud);

        await model.Login.RunAsync(RxVoid.Default);
        var succeeded = (model.HasErrorMessage, model.IsBusy);
        _cloud.Operation = static _ => Task.FromException(new InvalidOperationException("closed"));
        await model.Login.RunAsync(RxVoid.Default);

        await Assert.That(succeeded).IsEqualTo((false, false));
        await Assert.That(_cloud.Calls).Contains("OAuth");
        await Assert.That(model.HasErrorMessage).IsTrue();
        await Assert.That(model.ErrorMessage).IsEqualTo("closed");
        _ = await Assert.That(static () => new OAuthViewModel(null!)).Throws<ArgumentNullException>();
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => _cloud.Dispose();
}
