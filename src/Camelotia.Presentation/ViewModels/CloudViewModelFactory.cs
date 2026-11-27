// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Creates the browser view model for a saved provider.</summary>
/// <param name="state">The saved browser state.</param>
/// <param name="provider">The provider to browse.</param>
/// <returns>The browser view model.</returns>
public delegate ICloudViewModel CloudViewModelFactory(CloudState state, ICloud provider);
