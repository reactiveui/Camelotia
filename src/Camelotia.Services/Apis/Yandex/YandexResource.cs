// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json.Serialization;

namespace Camelotia.Services.Apis.Yandex;

/// <summary>A file or folder on Yandex Disk.</summary>
/// <param name="Path">The path, prefixed with <c>disk:</c>.</param>
/// <param name="Type">Either <c>dir</c> or <c>file</c>.</param>
/// <param name="Name">The display name.</param>
/// <param name="Size">The size in bytes.</param>
/// <param name="Modified">The last change time.</param>
/// <param name="Embedded">One page of a folder's entries, or <see langword="null"/> for a file.</param>
[System.Diagnostics.DebuggerDisplay("YandexResource: {Path}")]
public sealed record YandexResource(
    string Path,
    string Type,
    string Name,
    long Size,
    DateTimeOffset Modified,
    [property: JsonPropertyName("_embedded")] YandexResourceList? Embedded);
