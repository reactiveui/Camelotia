// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>A VK document.</summary>
/// <param name="Id">The document identifier.</param>
/// <param name="OwnerId">The owner's user identifier.</param>
/// <param name="Title">The document title.</param>
/// <param name="Size">The size in bytes.</param>
/// <param name="Date">The upload time in Unix seconds.</param>
/// <param name="Url">The download address.</param>
[System.Diagnostics.DebuggerDisplay("VkDocument: {Id}")]
public sealed record VkDocument(long Id, long OwnerId, string Title, long Size, long Date, string Url);
