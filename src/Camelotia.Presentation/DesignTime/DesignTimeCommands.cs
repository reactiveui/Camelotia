// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Runtime.CompilerServices;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace Camelotia.Presentation.DesignTime;

/// <summary>Creates the do-nothing commands the design-time view models expose to the designer.</summary>
internal static class DesignTimeCommands
{
    /// <summary>Creates a command that does nothing.</summary>
    /// <returns>The command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReactiveCommand<RxVoid, RxVoid> Empty() => ReactiveCommand.Create(static () => { });

    /// <summary>Creates a command that returns a fixed value.</summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="value">The value each run returns.</param>
    /// <returns>The command.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReactiveCommand<RxVoid, T> Returning<T>(T value) => ReactiveCommand.Create(() => value);
}
