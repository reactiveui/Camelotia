// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using Camelotia.Services.Interfaces;

namespace Camelotia.Tests.Fakes;

/// <summary>An <see cref="IFileManager"/> that hands out in-memory streams instead of showing dialogs.</summary>
[System.Diagnostics.DebuggerDisplay("FakeFileManager: {ReadName}")]
public sealed class FakeFileManager : IFileManager
{
    /// <summary>Gets the content each read stream holds.</summary>
    public static ReadOnlyMemory<byte> Content { get; } = "abc"u8.ToArray();

    /// <summary>Gets or sets the file name the read dialog returns; empty means the user picked nothing.</summary>
    public string ReadName { get; set; } = "upload.txt";

    /// <summary>Gets the names the save dialog was asked for.</summary>
    public List<string> Written { get; } = [];

    /// <inheritdoc/>
    public ValueTask<Stream> OpenWrite(string name, CancellationToken cancellationToken)
    {
        Written.Add(name);
        return ValueTask.FromResult<Stream>(new MemoryStream());
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<(string Name, Stream Stream)> OpenRead(CancellationToken cancellationToken) =>
        ValueTask.FromResult<(string Name, Stream Stream)>((ReadName, new MemoryStream(Content.ToArray())));
}
