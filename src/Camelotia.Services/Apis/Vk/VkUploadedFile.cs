// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>The upload server's reply.</summary>
/// <param name="File">The value to pass to <c>docs.save</c>, or <see langword="null"/> when the upload failed.</param>
/// <param name="Error">The upload failure, or <see langword="null"/> when the upload worked.</param>
[System.Diagnostics.DebuggerDisplay("VkUploadedFile: {File}")]
public sealed record VkUploadedFile(string? File, string? Error);
