// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.Interfaces;
using Camelotia.Services.Models;

namespace Camelotia.Presentation.ViewModels;

/// <summary>Creates the view model for one listed entry.</summary>
/// <param name="file">The entry.</param>
/// <param name="provider">The provider that listed it.</param>
/// <returns>The view model.</returns>
public delegate IFileViewModel FileViewModelFactory(FileModel file, ICloudViewModel provider);
