// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
namespace Camelotia.Services.Models;

/// <summary>The storage providers Camelotia supports.</summary>
public enum CloudType
{
    /// <summary>An FTP server.</summary>
    Ftp = 0,

    /// <summary>The user's GitHub repositories.</summary>
    GitHub = 1,

    /// <summary>Google Drive.</summary>
    GoogleDrive = 2,

    /// <summary>The local file system.</summary>
    Local = 3,

    /// <summary>An SFTP server.</summary>
    Sftp = 4,

    /// <summary>VK documents.</summary>
    VkDocs = 5,

    /// <summary>Yandex Disk.</summary>
    Yandex = 6,
}
