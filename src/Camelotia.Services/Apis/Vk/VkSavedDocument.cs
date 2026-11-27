// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>The result of <c>docs.save</c>.</summary>
/// <param name="Type">The kind of saved file, such as <c>doc</c>.</param>
/// <param name="Doc">The saved document, set when <paramref name="Type"/> is <c>doc</c>.</param>
[System.Diagnostics.DebuggerDisplay("VkSavedDocument: {Type}")]
public sealed record VkSavedDocument(string Type, VkDocument? Doc);
