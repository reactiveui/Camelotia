// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Collections.ObjectModel;
using System.ComponentModel;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Interfaces;

/// <summary>Lists, adds, removes and selects the saved providers.</summary>
public interface IMainViewModel : INotifyPropertyChanged
{
    /// <summary>Gets the saved providers, newest first.</summary>
    ReadOnlyObservableCollection<ICloudViewModel> Clouds { get; }

    /// <summary>Gets or sets the selected provider, or <see langword="null"/> when none is selected.</summary>
    ICloudViewModel? SelectedProvider { get; set; }

    /// <summary>Gets the provider types the user can add.</summary>
    IEnumerable<CloudType> SupportedTypes { get; }

    /// <summary>Gets or sets the provider type the add command creates.</summary>
    CloudType SelectedSupportedType { get; set; }

    /// <summary>Gets a value indicating whether the welcome screen is hidden.</summary>
    bool WelcomeScreenCollapsed { get; }

    /// <summary>Gets a value indicating whether the welcome screen is shown.</summary>
    bool WelcomeScreenVisible { get; }

    /// <summary>Gets the command that clears the selection.</summary>
    ReactiveCommand<RxVoid, RxVoid> Unselect { get; }

    /// <summary>Gets the command that reloads the provider list.</summary>
    ReactiveCommand<RxVoid, RxVoid> Refresh { get; }

    /// <summary>Gets the command that removes the selected provider.</summary>
    ReactiveCommand<RxVoid, RxVoid> Remove { get; }

    /// <summary>Gets the command that adds a provider of the selected type.</summary>
    ReactiveCommand<RxVoid, RxVoid> Add { get; }

    /// <summary>Gets a value indicating whether the list is reloading.</summary>
    bool IsLoading { get; }

    /// <summary>Gets a value indicating whether the list is ready.</summary>
    bool IsReady { get; }
}
