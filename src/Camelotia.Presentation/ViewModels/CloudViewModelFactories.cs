// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Presentation.ViewModels;

/// <summary>The factories a <see cref="CloudViewModel"/> uses to create its child view models.</summary>
/// <param name="CreateFolder">Creates the create-folder form.</param>
/// <param name="Rename">Creates the rename form.</param>
/// <param name="File">Creates the view model for each listed entry.</param>
/// <param name="Folder">Creates the view model for each breadcrumb folder.</param>
[System.Diagnostics.DebuggerDisplay("CloudViewModelFactories")]
public sealed record CloudViewModelFactories(
    CreateFolderViewModelFactory CreateFolder,
    RenameFileViewModelFactory Rename,
    FileViewModelFactory File,
    FolderViewModelFactory Folder);
