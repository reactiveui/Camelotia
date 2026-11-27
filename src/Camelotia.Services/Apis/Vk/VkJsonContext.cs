// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Camelotia.Services.Apis.Vk;

/// <summary>The generated JSON metadata for the VK replies. VK names its fields in snake case.</summary>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(VkReply<VkList<VkDocument>>))]
[JsonSerializable(typeof(VkReply<VkUploadServer>))]
[JsonSerializable(typeof(VkReply<VkSavedDocument>))]
[JsonSerializable(typeof(VkUploadedFile))]
[JsonSerializable(typeof(VkToken))]
internal sealed partial class VkJsonContext : JsonSerializerContext;
