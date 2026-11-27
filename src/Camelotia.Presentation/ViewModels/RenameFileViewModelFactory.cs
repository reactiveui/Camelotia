// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Creates the rename form for a provider view model.</summary>
/// <param name="providerViewModel">The provider view model the form belongs to.</param>
/// <returns>The form view model.</returns>
public delegate IRenameFileViewModel RenameFileViewModelFactory(ICloudViewModel providerViewModel);
