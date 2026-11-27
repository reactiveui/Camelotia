// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Collections.ObjectModel;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;

namespace Camelotia.Presentation.DesignTime;

/// <summary>A sample provider list for the designer.</summary>
[System.Diagnostics.DebuggerDisplay("DesignTimeMainViewModel: {SelectedSupportedType}")]
public sealed partial class DesignTimeMainViewModel : ReactiveObject, IMainViewModel, IDisposable
{
    /// <summary>Backs the first item of <see cref="Clouds"/>.</summary>
    private readonly DesignTimeCloudViewModel _first = new();

    /// <summary>Backs the second item of <see cref="Clouds"/>.</summary>
    private readonly DesignTimeCloudViewModel _second = new();

    /// <summary>Initializes a new instance of the <see cref="DesignTimeMainViewModel"/> class.</summary>
    public DesignTimeMainViewModel()
    {
        Clouds = new(new ObservableCollection<ICloudViewModel>([_first, _second]));
        SelectedProvider = _first;
    }

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ICloudViewModel> Clouds { get; }

    /// <inheritdoc/>
    [Reactive]
    public partial ICloudViewModel? SelectedProvider { get; set; }

    /// <inheritdoc/>
    public IEnumerable<CloudType> SupportedTypes { get; } = [CloudType.Ftp, CloudType.Sftp];

    /// <inheritdoc/>
    [Reactive]
    public partial CloudType SelectedSupportedType { get; set; } = CloudType.Sftp;

    /// <inheritdoc/>
    public bool WelcomeScreenCollapsed => true;

    /// <inheritdoc/>
    public bool WelcomeScreenVisible => false;

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Unselect { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Refresh { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Remove { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public ReactiveCommand<RxVoid, RxVoid> Add { get; } = DesignTimeCommands.Empty();

    /// <inheritdoc/>
    public bool IsLoading => false;

    /// <inheritdoc/>
    public bool IsReady => true;

    /// <inheritdoc/>
    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
        Unselect.Dispose();
        Refresh.Dispose();
        Remove.Dispose();
        Add.Dispose();
    }
}
