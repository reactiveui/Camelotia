// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Refit;

namespace Camelotia.Services.Apis.Vk;

/// <summary>The VK API documents methods at <c>https://api.vk.ru</c>.</summary>
public interface IVkApi
{
    /// <summary>Lists the user's documents.</summary>
    /// <param name="ownerId">The user identifier.</param>
    /// <param name="token">The access token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The documents.</returns>
    [Get($"/method/docs.get?v={VkApiVersion.Value}")]
    ValueTask<VkReply<VkList<VkDocument>>> GetDocumentsAsync([AliasAs("owner_id")] long ownerId, [Authorize] string token, CancellationToken cancellationToken);

    /// <summary>Gets the address to upload a document to. The document lands in the user's sent documents.</summary>
    /// <param name="token">The access token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The upload server.</returns>
    [Get($"/method/docs.getWallUploadServer?v={VkApiVersion.Value}")]
    ValueTask<VkReply<VkUploadServer>> GetUploadServerAsync([Authorize] string token, CancellationToken cancellationToken);

    /// <summary>Saves an uploaded file as a document.</summary>
    /// <param name="file">The file value the upload server returned.</param>
    /// <param name="title">The document title.</param>
    /// <param name="token">The access token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The saved document.</returns>
    [Post($"/method/docs.save?v={VkApiVersion.Value}")]
    ValueTask<VkReply<VkSavedDocument>> SaveAsync([Query] string file, [Query] string title, [Authorize] string token, CancellationToken cancellationToken);

    /// <summary>Sends a file to the address from <see cref="GetUploadServerAsync"/>.</summary>
    /// <param name="uploadUrl">The absolute upload address.</param>
    /// <param name="file">The file part, sent under the field name <c>file</c>.</param>
    /// <param name="cancellationToken">Cancels the upload.</param>
    /// <returns>The uploaded file value to pass to <see cref="SaveAsync"/>.</returns>
    [Multipart]
    [Post("")]
    ValueTask<VkUploadedFile> UploadAsync([Url] string uploadUrl, [AliasAs("file")] StreamPart file, CancellationToken cancellationToken);

    /// <summary>Downloads a document.</summary>
    /// <param name="url">The absolute document address.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>The response stream, which the caller disposes.</returns>
    [Get("")]
    ValueTask<Stream> DownloadAsync([Url] string url, CancellationToken cancellationToken);
}
