// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json.Serialization;

namespace Camelotia.Services.Apis.Yandex;

/// <summary>A Yandex OAuth token reply.</summary>
/// <param name="AccessToken">The access token.</param>
[System.Diagnostics.DebuggerDisplay("YandexToken: {AccessToken}")]
public sealed record YandexToken([property: JsonPropertyName("access_token")] string AccessToken);
