// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Yandex;

/// <summary>A one-off link for an upload or download.</summary>
/// <param name="Href">The absolute link.</param>
[System.Diagnostics.DebuggerDisplay("YandexLink: {Href}")]
public sealed record YandexLink(string Href);
