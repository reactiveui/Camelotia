// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Camelotia.Services.Apis.Yandex;

/// <summary>The generated JSON metadata for the Yandex replies.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(YandexResource))]
[JsonSerializable(typeof(YandexLink))]
[JsonSerializable(typeof(YandexToken))]
internal sealed partial class YandexJsonContext : JsonSerializerContext;
