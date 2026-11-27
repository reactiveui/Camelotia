// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Interfaces;

/// <summary>Asks the user to pick local files to upload or save.</summary>
public interface IFileManager
{
    /// <summary>Asks where to save a file and opens it for writing.</summary>
    /// <param name="name">The suggested file name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The stream to write to.</returns>
    ValueTask<Stream> OpenWrite(string name, CancellationToken cancellationToken);

    /// <summary>Asks which file to upload and opens it for reading.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The file name and the stream to read from.</returns>
    ValueTask<(string Name, Stream Stream)> OpenRead(CancellationToken cancellationToken);
}
