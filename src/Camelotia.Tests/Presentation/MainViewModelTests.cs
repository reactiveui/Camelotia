// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Models;
using Camelotia.Tests.Fakes;
using Camelotia.Tests.Infrastructure;
using DynamicData;
using ReactiveUI.Primitives;

namespace Camelotia.Tests.Presentation;

/// <summary>Tests <see cref="MainViewModel"/> over a fake cloud factory.</summary>
[System.Diagnostics.DebuggerDisplay("MainViewModelTests")]
public sealed class MainViewModelTests : IDisposable
{
    /// <summary>The number of providers the ordering test saves.</summary>
    private const int ProviderCount = 3;

    /// <summary>The saved app state.</summary>
    private readonly MainState _state = new();

    /// <summary>Creates a fake provider for each saved one.</summary>
    private readonly FakeCloudFactory _factory = new();

    /// <summary>Lists the saved providers newest first, and selects the saved selection.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Clouds_ListNewestFirstAndRestoreTheSelection()
    {
        var newest = new CloudState { Created = TestValues.Moment.AddDays(ProviderCount) };
        _state.Clouds.AddOrUpdate([new CloudState { Created = TestValues.Moment }, newest, new CloudState { Created = TestValues.Moment.AddDays(1) }]);
        _state.SelectedProviderId = newest.Id;
        using var model = Create();

        await Assert.That(model.Clouds.Count).IsEqualTo(ProviderCount);
        await Assert.That(model.Clouds[0].Id).IsEqualTo(newest.Id);
        await Assert.That(model.Clouds[^1].Created).IsEqualTo(TestValues.Moment);
        await Assert.That(model.SelectedProvider!.Id).IsEqualTo(newest.Id);
        await Assert.That(model.WelcomeScreenVisible).IsFalse();
        await Assert.That(model.WelcomeScreenCollapsed).IsTrue();
    }

    /// <summary>Adds a provider of the selected type with the current time, and saves the type.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Add_CreatesAProviderOfTheSelectedType()
    {
        using var model = Create();
        var defaultType = model.SelectedSupportedType;

        model.SelectedSupportedType = CloudType.Ftp;
        await model.Add.RunAsync(RxVoid.Default);

        await Assert.That(defaultType).IsEqualTo(CloudType.Local);
        await Assert.That(model.Clouds.Count).IsEqualTo(1);
        await Assert.That(_factory.Created[^1].Parameters.Type).IsEqualTo(CloudType.Ftp);
        await Assert.That(model.Clouds[0].Created).IsEqualTo(TestValues.Moment);
        await Assert.That(_state.SelectedSupportedType).IsEqualTo(CloudType.Ftp);
        await Assert.That(model.SupportedTypes).IsEquivalentTo(_factory.SupportedClouds);
    }

    /// <summary>Restores the saved provider type.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task SelectedSupportedType_RestoresTheSavedType()
    {
        _state.SelectedSupportedType = CloudType.Ftp;

        using var model = Create();

        await Assert.That(model.SelectedSupportedType).IsEqualTo(CloudType.Ftp);
    }

    /// <summary>Saves the selection, unselects it, and removes the selected provider.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Selection_IsSavedUnselectedAndRemoved()
    {
        _state.Clouds.AddOrUpdate(new CloudState());
        using var model = Create();
        var canRemoveNothing = model.Remove.CanRunNow();

        model.SelectedProvider = model.Clouds[0];
        var savedId = _state.SelectedProviderId;
        await model.Unselect.RunAsync(RxVoid.Default);
        var afterUnselect = model.SelectedProvider;
        model.SelectedProvider = model.Clouds[0];
        await model.Remove.RunAsync(RxVoid.Default);

        await Assert.That(canRemoveNothing).IsFalse();
        await Assert.That(savedId).IsEqualTo(_factory.Created[0].Id);
        await Assert.That(afterUnselect).IsNull();
        await Assert.That(model.Clouds).IsEmpty();
        await Assert.That(model.SelectedProvider).IsNull();
        await Assert.That(_state.SelectedProviderId).IsEqualTo(Guid.Empty);
    }

    /// <summary>Reloads the providers and reports when it is ready.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Refresh_ReloadsTheProviders()
    {
        _state.Clouds.AddOrUpdate(new CloudState());
        using var model = Create();

        await model.Refresh.RunAsync(RxVoid.Default);

        await Assert.That(model.IsReady).IsTrue();
        await Assert.That(model.IsLoading).IsFalse();
        await Assert.That(model.Clouds.Count).IsEqualTo(1);
    }

    /// <summary>Rejects missing dependencies, and a factory with no provider types.</summary>
    /// <returns>A task that completes when the test finishes.</returns>
    [Test]
    public async Task Constructor_RejectsBadDependencies()
    {
        using var empty = new FakeCloudFactory([]);

        _ = await Assert.That(() => new MainViewModel(null!, _factory, CreateCloudViewModel, TimeProvider.System)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new MainViewModel(_state, null!, CreateCloudViewModel, TimeProvider.System)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new MainViewModel(_state, _factory, CreateCloudViewModel, null!)).Throws<ArgumentNullException>();
        _ = await Assert.That(() => new MainViewModel(new(), empty, CreateCloudViewModel, TimeProvider.System)).Throws<InvalidOperationException>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _state.Dispose();
        _factory.Dispose();
    }

    /// <summary>Creates a browser view model for a saved provider.</summary>
    /// <param name="state">The provider's saved state.</param>
    /// <param name="provider">The provider.</param>
    /// <returns>The browser view model.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CloudViewModel CreateCloudViewModel(CloudState state, Camelotia.Services.Interfaces.ICloud provider) =>
        ViewModelBuilder.CreateCloudViewModel(state, provider, new FakeFileManager());

    /// <summary>Creates the view model with a clock fixed at <see cref="TestValues.Moment"/>.</summary>
    /// <returns>The view model.</returns>
    private MainViewModel Create() => new(_state, _factory, CreateCloudViewModel, new FixedTimeProvider(TestValues.Moment));

    /// <summary>A clock that always reads the same time.</summary>
    /// <param name="now">The time it reads.</param>
    [System.Diagnostics.DebuggerDisplay("FixedTimeProvider")]
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        /// <inheritdoc/>
        public override DateTimeOffset GetUtcNow() => now;
    }
}
