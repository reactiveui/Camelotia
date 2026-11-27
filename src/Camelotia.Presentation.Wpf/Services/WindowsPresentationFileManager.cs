// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.IO;
using Camelotia.Services.Interfaces;
using Microsoft.Win32;

namespace Camelotia.Presentation.Wpf.Services;

/// <summary>Asks the user for local files with the WPF file and folder dialogs.</summary>
[System.Diagnostics.DebuggerDisplay("WindowsPresentationFileManager")]
public sealed class WindowsPresentationFileManager : IFileManager
{
    /// <summary>Asks for a file and opens it for reading.</summary>
    /// <param name="cancellationToken">Cancels the request before the dialog opens.</param>
    /// <returns>The file name and stream, or an empty name and <see cref="Stream.Null"/> when the user chose nothing.</returns>
    public ValueTask<(string Name, Stream Stream)> OpenRead(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dialog = new OpenFileDialog();
        return dialog.ShowDialog() == true
            ? new((Path.GetFileName(dialog.FileName), File.OpenRead(dialog.FileName)))
            : new((string.Empty, Stream.Null));
    }

    /// <summary>Asks for a folder and creates a file with the given name in it.</summary>
    /// <param name="name">The file name.</param>
    /// <param name="cancellationToken">Cancels the request before the dialog opens.</param>
    /// <returns>The stream to write to.</returns>
    /// <exception cref="OperationCanceledException">The user closed the dialog without choosing a folder.</exception>
    public ValueTask<Stream> OpenWrite(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dialog = new OpenFolderDialog();
        if (dialog.ShowDialog() != true)
        {
            throw new OperationCanceledException("No folder was chosen.");
        }

        return new(File.Create(Path.Combine(dialog.FolderName, name)));
    }
}
