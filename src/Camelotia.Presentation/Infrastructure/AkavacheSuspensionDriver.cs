// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Akavache;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.Infrastructure;

/// <summary>Saves the app state in an Akavache cache, serialized with source-generated JSON metadata.</summary>
/// <param name="cache">The cache that holds the state.</param>
[System.Diagnostics.DebuggerDisplay("AkavacheSuspensionDriver")]
public sealed class AkavacheSuspensionDriver(IBlobCache cache) : ISuspensionDriver
{
    /// <summary>The cache key the state is saved under.</summary>
    private const string Key = "camelotia-state";

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<RxVoid> InvalidateState() => cache.Invalidate(Key);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<T?> LoadState<T>(JsonTypeInfo<T> typeInfo) =>
        cache.Get(Key).Select(bytes => bytes is null ? default : JsonSerializer.Deserialize(bytes, typeInfo));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<RxVoid> SaveState<T>(T state, JsonTypeInfo<T> typeInfo) =>
        cache.Insert(Key, JsonSerializer.SerializeToUtf8Bytes(state, typeInfo));
}
