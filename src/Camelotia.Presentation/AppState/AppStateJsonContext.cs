// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Camelotia.Presentation.AppState;

/// <summary>The generated JSON metadata the suspension driver saves and loads <see cref="MainState"/> with.</summary>
[System.Diagnostics.DebuggerDisplay("AppStateJsonContext")]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(MainState))]
public sealed partial class AppStateJsonContext : JsonSerializerContext;
