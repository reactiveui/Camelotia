// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Services.Apis.Vk;
using Camelotia.Services.Apis.Yandex;

namespace Camelotia.Services.Apis;

/// <summary>The generated Refit clients the HTTP providers call.</summary>
/// <param name="YandexDisk">The Yandex Disk REST API.</param>
/// <param name="YandexOAuth">The Yandex OAuth server.</param>
/// <param name="Vk">The VK API documents methods.</param>
/// <param name="VkId">The VK ID OAuth server.</param>
[System.Diagnostics.DebuggerDisplay("CloudApis: {YandexDisk}")]
public sealed record CloudApis(IYandexDiskApi YandexDisk, IYandexOAuthApi YandexOAuth, IVkApi Vk, IVkOAuthApi VkId);
