// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Camelotia.Services.Interfaces;

namespace Camelotia.Presentation.Avalonia.Services;

/// <summary>Asks the user for local files with the Avalonia storage pickers.</summary>
/// <param name="window">The window the pickers belong to.</param>
[System.Diagnostics.DebuggerDisplay("AvaloniaFileManager")]
public sealed class AvaloniaFileManager(Window window) : IFileManager
{
    /// <summary>Asks for a folder and creates a file with the given name in it.</summary>
    /// <param name="name">The file name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The stream to write to.</returns>
    /// <exception cref="OperationCanceledException">The user closed the picker without choosing a folder.</exception>
    public async ValueTask<Stream> OpenWrite(string name, CancellationToken cancellationToken)
    {
        var folders = await window.StorageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false })
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        if (folders.Count == 0)
        {
            throw new OperationCanceledException("No folder was chosen.");
        }

        return File.Create(Path.Combine(folders[0].Path.LocalPath, name));
    }

    /// <summary>Asks for a file and opens it for reading.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The file name and stream, or an empty name and <see cref="Stream.Null"/> when the user chose nothing.</returns>
    public async ValueTask<(string Name, Stream Stream)> OpenRead(CancellationToken cancellationToken)
    {
        var files = await window.StorageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false })
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        if (files.Count == 0)
        {
            return (string.Empty, Stream.Null);
        }

        var stream = await files[0].OpenReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
        return (files[0].Name, stream);
    }
}
