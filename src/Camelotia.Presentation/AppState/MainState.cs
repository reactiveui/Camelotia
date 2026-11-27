// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Camelotia.Services.Configuration;
using Camelotia.Services.Models;
using DynamicData;

namespace Camelotia.Presentation.AppState;

/// <summary>The app state the suspension driver saves between runs.</summary>
[System.Diagnostics.DebuggerDisplay("MainState: {SelectedProviderId}")]
[DataContract]
public sealed class MainState : IDisposable
{
    /// <summary>Gets the saved providers, keyed by identifier.</summary>
    [IgnoreDataMember]
    [JsonIgnore]
    public SourceCache<CloudState, Guid> Clouds { get; } = new(static x => x.Id);

    /// <summary>Gets or sets the saved providers, for serialization. Setting the value adds or updates each provider.</summary>
    [DataMember]
    public IEnumerable<CloudState> CloudStates
    {
        get => Clouds.Items;
        set => Clouds.AddOrUpdate(value);
    }

    /// <summary>Gets or sets the provider type the add menu shows.</summary>
    [DataMember]
    public CloudType? SelectedSupportedType { get; set; }

    /// <summary>Gets or sets the identifier of the selected provider, or <see cref="Guid.Empty"/> when none is selected.</summary>
    [DataMember]
    public Guid SelectedProviderId { get; set; }

    /// <summary>Gets or sets the application keys for each provider.</summary>
    [DataMember]
    public CloudConfiguration CloudConfiguration { get; set; } = new();

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Clouds.Dispose();
}
