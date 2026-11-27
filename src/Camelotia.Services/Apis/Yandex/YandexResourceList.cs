// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Apis.Yandex;

/// <summary>One page of a Yandex Disk folder's entries.</summary>
/// <param name="Items">The entries on this page.</param>
/// <param name="Total">The number of entries in the folder.</param>
[System.Diagnostics.DebuggerDisplay("YandexResourceList: {Items}")]
public sealed record YandexResourceList(List<YandexResource> Items, int Total);
