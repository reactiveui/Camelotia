// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Vk;

/// <summary>A counted list VK returns.</summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Count">The total number of items.</param>
/// <param name="Items">The items in this reply.</param>
[System.Diagnostics.DebuggerDisplay("VkList: {Count}")]
public sealed record VkList<T>(int Count, List<T> Items);
