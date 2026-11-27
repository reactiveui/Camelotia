// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Collections.ObjectModel;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.SourceGenerators;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Lists the saved providers, newest first, and adds, removes and selects them.</summary>
[System.Diagnostics.DebuggerDisplay("MainViewModel: {SelectedSupportedType}")]
public sealed partial class MainViewModel : ReactiveObject, IMainViewModel, IDisposable
{
    /// <summary>Holds the subscriptions that live as long as the view model.</summary>
    private readonly MultipleDisposable _subscriptions = new();

    /// <summary>Backs <see cref="Clouds"/>.</summary>
    private readonly ReadOnlyObservableCollection<ICloudViewModel> _providers;

    /// <summary>Backs <see cref="WelcomeScreenCollapsed"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _welcomeScreenCollapsed;

    /// <summary>Backs <see cref="WelcomeScreenVisible"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _welcomeScreenVisible;

    /// <summary>Backs <see cref="IsLoading"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isLoading;

    /// <summary>Backs <see cref="IsReady"/>.</summary>
    private readonly ObservableAsPropertyHelper<bool> _isReady;

    /// <summary>Creates providers from saved parameters.</summary>
    private readonly ICloudFactory _factory;

    /// <summary>Initializes a new instance of the <see cref="MainViewModel"/> class.</summary>
    /// <param name="state">The saved app state.</param>
    /// <param name="factory">Creates providers from saved parameters.</param>
    /// <param name="createViewModel">Creates the browser view model for each provider.</param>
    /// <param name="timeProvider">Supplies the time a new provider is added.</param>
    public MainViewModel(MainState state, ICloudFactory factory, CloudViewModelFactory createViewModel, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _factory = factory;
        Refresh = ReactiveCommand.Create(state.Clouds.Refresh);

        _isLoading = Refresh.IsExecuting.ToProperty(this, static x => x.IsLoading);

        _isReady = Refresh
            .IsExecuting
            .Select(static executing => !executing)
            .ToProperty(this, static x => x.IsReady);

        _subscriptions.Add(state
            .Clouds
            .Connect()
            .Transform(cloud => createViewModel(cloud, factory.CreateCloud(cloud.Parameters)))
            .DisposeMany()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(out _providers, SortExpressionComparer<ICloudViewModel>.Descending(static x => x.Created))
            .Subscribe());

        var canRemove = this
            .WhenAnyValue(static x => x.SelectedProvider)
            .Select(static provider => provider is not null);

        Remove = ReactiveCommand.Create(
            () => state.Clouds.RemoveKey((SelectedProvider ?? throw new InvalidOperationException("Select a provider to remove.")).Id),
            canRemove);

        var canAddProvider = this
            .WhenAnyValue(static x => x.SelectedSupportedType)
            .Select(Enum.IsDefined);

        Add = ReactiveCommand.Create(
            () => state.Clouds.AddOrUpdate(new CloudState { Type = SelectedSupportedType, Created = timeProvider.GetUtcNow() }),
            canAddProvider);

        _welcomeScreenVisible = this
            .WhenAnyValue(static x => x.SelectedProvider)
            .Select(static provider => provider is null)
            .ToProperty(this, static x => x.WelcomeScreenVisible);

        _welcomeScreenCollapsed = this
            .WhenAnyValue(static x => x.WelcomeScreenVisible)
            .Select(static visible => !visible)
            .ToProperty(this, static x => x.WelcomeScreenCollapsed);

        var canUnselect = this
            .WhenAnyValue(static x => x.SelectedProvider)
            .Select(static provider => provider is not null);

        Unselect = ReactiveCommand.Create(static () => RxVoid.Default, canUnselect);
        WireSelection(state);
    }

    /// <inheritdoc/>
    [Reactive]
    public partial CloudType SelectedSupportedType { get; set; }

    /// <inheritdoc/>
    [Reactive]
    public partial ICloudViewModel? SelectedProvider { get; set; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Unselect { get; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Refresh { get; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Remove { get; }

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Add { get; }

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ICloudViewModel> Clouds => _providers;

    /// <inheritdoc/>
    public IEnumerable<CloudType> SupportedTypes => _factory.SupportedClouds;

    /// <inheritdoc/>
    public bool WelcomeScreenCollapsed => _welcomeScreenCollapsed.Value;

    /// <inheritdoc/>
    public bool WelcomeScreenVisible => _welcomeScreenVisible.Value;

    /// <inheritdoc/>
    public bool IsLoading => _isLoading.Value;

    /// <inheritdoc/>
    public bool IsReady => _isReady.Value;

    /// <inheritdoc/>
    public void Dispose()
    {
        _subscriptions.Dispose();
        _welcomeScreenCollapsed.Dispose();
        _welcomeScreenVisible.Dispose();
        _isLoading.Dispose();
        _isReady.Dispose();
        Unselect.Dispose();
        Refresh.Dispose();
        Remove.Dispose();
        Add.Dispose();
    }

    /// <summary>Gets the first provider type the factory supports.</summary>
    /// <returns>The provider type.</returns>
    /// <exception cref="InvalidOperationException">The factory supports no provider types.</exception>
    private CloudType FirstSupportedType()
    {
        using var types = _factory.SupportedClouds.GetEnumerator();
        return types.MoveNext() ? types.Current : throw new InvalidOperationException("The cloud factory supports no provider types.");
    }

    /// <summary>Keeps the selection and the saved state in step.</summary>
    /// <param name="state">The saved app state.</param>
    private void WireSelection(MainState state)
    {
        _subscriptions.Add(Unselect.Subscribe(_ => SelectedProvider = null));

        var outputCollectionChanges = Clouds
            .ToObservableChangeSet(static x => x.Id)
            .Publish()
            .RefCount();

        _subscriptions.Add(outputCollectionChanges
            .Filter(provider => provider.Id == state.SelectedProviderId)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .OnItemAdded(provider => SelectedProvider = provider)
            .Subscribe());

        _subscriptions.Add(outputCollectionChanges
            .OnItemRemoved(_ => SelectedProvider = null)
            .Subscribe());

        _subscriptions.Add(this.WhenAnyValue(static x => x.SelectedProvider)
            .Skip(1)
            .Select(static provider => provider?.Id ?? Guid.Empty)
            .Subscribe(id => state.SelectedProviderId = id));

        SelectedSupportedType = state.SelectedSupportedType ?? FirstSupportedType();
        _subscriptions.Add(this.WhenAnyValue(static x => x.SelectedSupportedType)
            .Subscribe(type => state.SelectedSupportedType = type));
    }
}
