// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Refit;

namespace Camelotia.Services.Apis.Yandex;

/// <summary>The Yandex Disk REST API at <c>https://cloud-api.yandex.net</c>.</summary>
public interface IYandexDiskApi
{
    /// <summary>Reads a folder and one page of its entries.</summary>
    /// <param name="path">The folder path on the disk.</param>
    /// <param name="limit">The most entries to return. Yandex returns 20 when this is not sent.</param>
    /// <param name="offset">The number of entries to skip.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The folder resource.</returns>
    [Get("/v1/disk/resources")]
    ValueTask<YandexResource> GetResourceAsync([Query] string path, [Query] int limit, [Query] int offset, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Creates a folder.</summary>
    /// <param name="path">The new folder path.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the folder exists.</returns>
    [Put("/v1/disk/resources")]
    Task CreateFolderAsync([Query] string path, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Deletes a file or folder.</summary>
    /// <param name="path">The entry path.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the entry is gone.</returns>
    [Delete("/v1/disk/resources")]
    Task DeleteAsync([Query] string path, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Moves or renames a file or folder.</summary>
    /// <param name="source">The current path.</param>
    /// <param name="path">The new path.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>A task that completes when the entry has moved.</returns>
    [Post("/v1/disk/resources/move")]
    Task MoveAsync([Query][AliasAs("from")] string source, [Query] string path, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Gets a one-off link to download a file from.</summary>
    /// <param name="path">The file path.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The download link.</returns>
    [Get("/v1/disk/resources/download")]
    ValueTask<YandexLink> GetDownloadLinkAsync([Query] string path, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Gets a one-off link to upload a file to.</summary>
    /// <param name="path">The file path to create.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The upload link.</returns>
    [Get("/v1/disk/resources/upload")]
    ValueTask<YandexLink> GetUploadLinkAsync([Query] string path, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Downloads the content behind a link from <see cref="GetDownloadLinkAsync"/>. Yandex wants the same token as the link request.</summary>
    /// <param name="link">The absolute download link.</param>
    /// <param name="token">The OAuth token.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>The response stream, which the caller disposes.</returns>
    [Get("")]
    ValueTask<Stream> DownloadAsync([Url] string link, [Authorize("OAuth")] string token, CancellationToken cancellationToken);

    /// <summary>Uploads content to a link from <see cref="GetUploadLinkAsync"/>. The link needs no token.</summary>
    /// <param name="link">The absolute upload link.</param>
    /// <param name="content">The file content, which the caller owns.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>A task that completes when the upload finishes.</returns>
    [Put("")]
    Task UploadAsync([Url] string link, [Body] Stream content, CancellationToken cancellationToken);
}
