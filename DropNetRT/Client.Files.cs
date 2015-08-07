﻿using System.Threading;
using DropNetRT.Exceptions;
using DropNetRT.Extensions;
using DropNetRT.HttpHelpers;
using DropNetRT.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DropNetRT
{
    public partial class DropNetClient
    {

        /// <summary>
        /// Gets MetaData for a file or folder given the path
        /// </summary>
        /// <param name="path">Path to file or folder</param>
        /// <returns><see cref="Metadata"/> for a file or folder</returns>
        public async Task<Metadata> GetMetaData(string path, string hash = null)
        {
            return await GetMetaData(path, hash, null, true, false);
        }

        /// <summary>
        /// Shorthand for a GetMetadata call without contents listing
        /// </summary>
        /// <param name="path">Path to file or folder</param>
        /// <returns><see cref="Metadata"/> for a file or folder</returns>
        public async Task<Metadata> GetMetaDataNoList(string path, string hash = null)
        {
            return await GetMetaData(path, hash, null, false, false);
        }

        /// <summary>
        /// Shorthand for a GetMetadata call with deleted files
        /// </summary>
        /// <param name="path">Path to file or folder</param>
        /// <returns><see cref="Metadata"/> for a file or folder</returns>
        public async Task<Metadata> GetMetaDataWithDeleted(string path, string hash = null)
        {
            return await GetMetaData(path, hash, null, true, true);
        }

        /// <summary>
        /// Gets MetaData for a file or Folder (All options)
        /// </summary>
        /// <param name="path">Path to file or folder</param>
        /// <param name="hash">Each call to /metadata on a folder will return a hash field, generated by hashing all of the metadata contained in that response. On later calls to /metadata, you should provide that value via this parameter so that if nothing has changed, the response will be a 304 (Not Modified)</param>
        /// <param name="rev">If you include a particular revision number, then only the metadata for that revision will be returned.</param>
        /// <param name="list"> If true, the folder's metadata will include a contents field with a list of metadata entries for the contents of the folder. If false, the contents field will be omitted.</param>
        /// <param name="includeDeleted">Only applicable when list is set. If this parameter is set to true, then contents will include the metadata of deleted children. Note that the target of the metadata call is always returned even when it has been deleted (with is_deleted set to true) regardless of this flag.</param>
        /// <returns></returns>
        public Task<Metadata> GetMetaData(string path, string hash, int? rev, bool list, bool includeDeleted)
        {
            return GetMetaData(path, hash, rev, list, includeDeleted, null, CancellationToken.None);
        }

        /// <summary>
        /// Gets MetaData for a file or Folder (All options)
        /// </summary>
        /// <param name="path">Path to file or folder</param>
        /// <param name="hash">Each call to /metadata on a folder will return a hash field, generated by hashing all of the metadata contained in that response. On later calls to /metadata, you should provide that value via this parameter so that if nothing has changed, the response will be a 304 (Not Modified)</param>
        /// <param name="rev">If you include a particular revision number, then only the metadata for that revision will be returned.</param>
        /// <param name="list"> If true, the folder's metadata will include a contents field with a list of metadata entries for the contents of the folder. If false, the contents field will be omitted.</param>
        /// <param name="includeDeleted">Only applicable when list is set. If this parameter is set to true, then contents will include the metadata of deleted children. Note that the target of the metadata call is always returned even when it has been deleted (with is_deleted set to true) regardless of this flag.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> GetMetaData(string path, string hash, int? rev, bool list, bool includeDeleted, bool? includeMembership, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString(string.Format("1/metadata/{0}/{1}", Root, path.CleanPath()), ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            if (!string.IsNullOrEmpty(hash))
            {
                request.Parameters.Add(new HttpParameter("hash", hash));
            }
            request.Parameters.Add(new HttpParameter("list", list));
            request.Parameters.Add(new HttpParameter("include_deleted", includeDeleted));
            if (rev.HasValue)
            {
                request.Parameters.Add(new HttpParameter("rev", rev));
            }
            if (includeMembership.HasValue)
            {
                request.Parameters.Add(new HttpParameter("include_membership", includeMembership.Value));
            }

            var response = await SendAsync<Metadata>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Gets a share link from a give path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<ShareResponse> GetShare(string path)
        {
            return GetShare(path, CancellationToken.None);
        }

        /// <summary>
        /// Gets a share link from a give path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShareResponse> GetShare(string path, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString(string.Format("1/shares/{0}/{1}", Root, path.CleanPath()), ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);

            var response = await SendAsync<ShareResponse>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Gets a share link from a give path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        public Task<ShareResponse> GetShare(string path, bool shortUrl)
        {
            return GetShare(path, shortUrl, CancellationToken.None);
        }

        /// <summary>
        /// Gets a share link from a give path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="shortUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShareResponse> GetShare(string path, bool shortUrl, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString(string.Format("1/shares/{0}/{1}", Root, path.CleanPath()), ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.Parameters.Add(new HttpParameter("short_url", shortUrl));

            var response = await SendAsync<ShareResponse>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Searches for a given text in the entire dropbox/sandbox folder
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public async Task<List<Metadata>> Search(string searchString)
        {
            return await Search(searchString, string.Empty);
        }

        /// <summary>
        /// Searches for a given text in the entire dropbox/sandbox folder
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Metadata>> Search(string searchString, CancellationToken cancellationToken)
        {
            return await Search(searchString, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Searches for a given text in a specified folder
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<List<Metadata>> Search(string searchString, string path)
        {
            return Search(searchString, path, CancellationToken.None);
        }

        /// <summary>
        /// Searches for a given text in a specified folder
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Metadata>> Search(string searchString, string path, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString(string.Format("1/search/{0}/{1}", Root, path.CleanPath()), ApiType.Base);
            
            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.Parameters.Add(new HttpParameter("query", searchString));

            var response = await SendAsync<List<Metadata>>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Gets a file from the given path
        /// </summary>
        /// <param name="path">Path of the file in Dropbox to go</param>
        /// <returns></returns>
        public Task<byte[]> GetFile(string path)
        {
            return GetFile(path, CancellationToken.None);
        }

        /// <summary>
        /// Gets a file from the given path
        /// </summary>
        /// <param name="path">Path of the file in Dropbox to go</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFile(string path, CancellationToken cancellationToken)
        {
            var request = MakeGetFileRequest(path);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            //TODO - Error Handling

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Gets a file stream from the given path
        /// </summary>
        /// <param name="path">Path of the file in Dropbox to go</param>
        /// <returns></returns>
        public Task<Stream> GetFileStream(string path)
        {
            return GetFileStream(path, CancellationToken.None);
        }

        /// <summary>
        /// Gets a file stream from the given path
        /// </summary>
        /// <param name="path">Path of the file in Dropbox to go</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Stream> GetFileStream(string path, CancellationToken cancellationToken)
        {
            var request = MakeGetFileRequest(path);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            //TODO - Error Handling

            if (HttpStatusCode.OK != response.StatusCode)
            {
                throw new DropboxException(response.StatusCode);
            }

            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Gets a file download uri with user authentication added (for use with Background Transfers)
        /// </summary>
        /// <param name="path">Path of the file in Dropbox to go</param>
        /// <returns></returns>
        public Uri GetFileUrl(string path)
        {
            var request = MakeGetFileRequest(path);

            return request.RequestUri;
        }


        /// <summary>
        /// Gets the upload Uri for a file (for use with Background Transfers)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Uri UploadUrl(string path, string filename)
        {
            return UploadUrl(path, filename, null);
        }

        /// <summary>
        /// Gets the upload Uri for a file (for use with Background Transfers)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="parentRevision"></param>
        /// <returns></returns>
        public Uri UploadUrl(string path, string filename, string parentRevision)
        {
            var request = MakeUploadRequest(path, filename, parentRevision);

            return request.RequestUri;
        }

        /// <summary>
        /// Uploads a file to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public Task<Metadata> Upload(string path, string filename, byte[] fileData)
        {
            return Upload(path, filename, fileData, CancellationToken.None);
        }

        /// <summary>
        /// Uploads a file to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="fileData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Metadata> Upload(string path, string filename, byte[] fileData, CancellationToken cancellationToken)
        {
            return Upload(path, filename, fileData, null, cancellationToken);
        }

        /// <summary>
        /// Uploads a file to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="fileData"></param>
        /// <param name="parentRevision"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Upload(string path, string filename, byte[] fileData, string parentRevision, CancellationToken cancellationToken)
        {
            var request = MakeUploadRequest(path, filename, parentRevision);

            var content = new MultipartFormDataContent(_formBoundary);

            foreach (var parm in request.Parameters)
            {
                content.Add(new StringContent(parm.Value.ToString()), parm.Name);
            }

            var fileContent = new ByteArrayContent(fileData);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("file")
            {
                FileName = filename,
                Name = "file"
            };
            fileContent.Headers.Add("Content-Type", "application/octet-stream");
            content.Add(fileContent);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(request.RequestUri, content, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DropboxException(ex);
            }

            //TODO - More Error Handling
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new DropboxException(response);
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Metadata>(responseBody);
        }

        /// <summary>
        /// Uploads a streamed data to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Task<Metadata> Upload(string path, string filename, Stream stream)
        {
            return Upload(path, filename, stream, CancellationToken.None);
        }

        /// <summary>
        /// Uploads a streamed data to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Upload(string path, string filename, Stream stream, CancellationToken cancellationToken)
        {
            return await Upload(path, filename, stream, null, cancellationToken);
        }

        /// <summary>
        /// Uploads a streamed data to a Dropbox folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Upload(string path, string filename, Stream stream, string parentRevision, CancellationToken cancellationToken)
        {
            var request = MakeUploadPutRequest(path, filename, parentRevision);

            var content = new StreamContent(stream);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PutAsync(request.RequestUri, content, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DropboxException(ex);
            }

            //TODO - More Error Handling
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new DropboxException(response);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Metadata>(responseBody);
        }

        private const int ChunkSize = 1024 * 1024;

        private async Task<ChunkedUploadResponse> UploadChunk(Stream stream, ChunkedUploadResponse lastResponse, CancellationToken cancellationToken)
        {
            return await UploadChunk(stream, ChunkSize, lastResponse, cancellationToken);
        }

        public async Task<ChunkedUploadResponse> UploadChunk(Stream stream, int chunkSize, ChunkedUploadResponse lastResponse, CancellationToken cancellationToken)
        {
            if(lastResponse == null)
            {
                throw new ArgumentNullException("lastResponse");
            }

            var offset = lastResponse.Offset;
            var uploadId = lastResponse.UploadId;

            var request = MakeChunkedUploadPutRequest(offset, uploadId);

            var buffer = new byte[chunkSize];

            stream.Seek(lastResponse.Offset, SeekOrigin.Begin);
            var contentSize = stream.Read(buffer, 0, chunkSize);

            if(contentSize == 0)
            {
                // Nothing left to send
                return null;
            }

            HttpContent content = new ByteArrayContent(buffer, 0, contentSize);

            try
            {
                var response = await _httpClient.PutAsync(request.RequestUri, content, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DropboxException(response);
                }

                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChunkedUploadResponse>(body);
            }
            catch (AggregateException)
            {
                throw;
            }
            catch (DropboxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DropboxException(ex);
            }
        }

        private async Task<ChunkedUploadResponse> StartChunkedUpload(Stream stream, CancellationToken cancellationToken)
        {
            return await StartChunkedUpload(stream, ChunkSize, cancellationToken);
        }

        public async Task<ChunkedUploadResponse> StartChunkedUpload(Stream stream, int chunkSize, CancellationToken cancellationToken)
        {
            var request = MakeChunkedUploadPutRequest(0);

            var buffer = new byte[chunkSize];

            var contentSize = stream.Read(buffer, 0, chunkSize);

            HttpContent content = new ByteArrayContent(buffer, 0, contentSize);

            try
            {
                var response = await _httpClient.PutAsync(request.RequestUri, content, cancellationToken);

                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DropboxException(response);
                }

                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ChunkedUploadResponse>(body);
            }
            catch (AggregateException)
            {
                throw;
            }
            catch(DropboxException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new DropboxException(ex);
            }
        }

        public async Task<Metadata> UploadChunked(string path, string filename, Stream stream, CancellationToken cancellationToken, IProgress<long> progress = null)
        {
            try
            {
                // Upload the initial chunk so we can get the upload_id value
                var chunkedUploadResponse = await StartChunkedUpload(stream, cancellationToken);

                if(chunkedUploadResponse == null)
                {
                    throw new Exception("Initial chunk upload response was null.");
                }

                var uploadId = chunkedUploadResponse.UploadId;

                if(progress != null)
                {
                    progress.Report(chunkedUploadResponse.Offset);
                }

                // Keep uploading subsequent chunks until we don't have anything left to upload
                while(chunkedUploadResponse != null)
                {
                    chunkedUploadResponse = await UploadChunk(stream, chunkedUploadResponse, cancellationToken);

                    if(progress != null && chunkedUploadResponse != null)
                    {
                        progress.Report(chunkedUploadResponse.Offset);
                    }
                }

                // Commit the upload
                return await CommitChunkedUpload(path, filename, null, uploadId, cancellationToken);
            }
            catch(AggregateException)
            {
                throw;
            }
            catch(DropboxException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new DropboxException(ex);
            }
        }

        public async Task<Metadata> CommitChunkedUpload(string path, string filename, string parentRevision, string uploadId, CancellationToken cancellationToken)
        {
            try
            {
                var commitRequest = MakeChunkedUploadCommitRequest(path, filename, parentRevision, uploadId);

                var response = await _httpClient.PostAsync(commitRequest.RequestUri, null, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DropboxException(response);
                }

                var responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Metadata>(responseBody);
            }
            catch (AggregateException)
            {
                throw;
            }
            catch (DropboxException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DropboxException(ex);
            }
        }

        /// <summary>
        /// Deletes the file or folder from dropbox with the given path
        /// </summary>
        /// <param name="path">The Path of the file or folder to delete.</param>
        /// <returns></returns>
        public Task<Metadata> Delete(string path)
        {
            return Delete(path, CancellationToken.None);
        }

        /// <summary>
        /// Deletes the file or folder from dropbox with the given path
        /// </summary>
        /// <param name="path">The Path of the file or folder to delete.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Delete(string path, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString("1/fileops/delete", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.AddParameter("path", path);
            request.AddParameter("root", Root);

            var response = await SendAsync<Metadata>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Copies a file or folder on Dropbox
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        public Task<Metadata> Copy(string fromPath, string toPath)
        {
            return Copy(fromPath, toPath, CancellationToken.None);
        }

        /// <summary>
        /// Copies a file or folder on Dropbox
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Copy(string fromPath, string toPath, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString("1/fileops/copy", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.AddParameter("from_path", fromPath);
            request.AddParameter("to_path", toPath);
            request.AddParameter("root", Root);

            var response = await SendAsync<Metadata>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Moves a file or folder on Dropbox
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        public Task<Metadata> Move(string fromPath, string toPath)
        {
            return Move(fromPath, toPath, CancellationToken.None);
        }

        /// <summary>
        /// Moves a file or folder on Dropbox
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> Move(string fromPath, string toPath, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString("1/fileops/move", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.AddParameter("from_path", fromPath);
            request.AddParameter("to_path", toPath);
            request.AddParameter("root", Root);

            var response = await SendAsync<Metadata>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Created a new folder with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<Metadata> CreateFolder(string path)
        {
            return CreateFolder(path, CancellationToken.None);
        }

        /// <summary>
        /// Created a new folder with the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Metadata> CreateFolder(string path, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString("1/fileops/create_folder", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);
            request.AddParameter("path", path);
            request.AddParameter("root", Root);

            var response = await SendAsync<Metadata>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Returns a link directly to a file.
        /// Similar to /shares. The difference is that this bypasses the Dropbox webserver, used to provide a preview of the file, so that you can effectively stream the contents of your media.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<ShareResponse> GetMedia(string path)
        {
            return GetMedia(path, CancellationToken.None);
        }

        /// <summary>
        /// Returns a link directly to a file.
        /// Similar to /shares. The difference is that this bypasses the Dropbox webserver, used to provide a preview of the file, so that you can effectively stream the contents of your media.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShareResponse> GetMedia(string path, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString(string.Format("1/media/{0}/{1}", Root, path.CleanPath()), ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);

            var response = await SendAsync<ShareResponse>(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Gets the thumbnail of an image given its MetaData (default size = small)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<byte[]> GetThumbnail(Metadata file)
        {
            return await GetThumbnail(file.Path, ThumbnailSize.Small);
        }

        /// <summary>
        /// Gets the thumbnail of an image given its MetaData
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<byte[]> GetThumbnail(Metadata file, ThumbnailSize size)
        {
            return await GetThumbnail(file.Path, size);
        }

        /// <summary>
        /// Gets the thumbnail of an image given its path (default size = small)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<byte[]> GetThumbnail(string path)
        {
            return await GetThumbnail(path, ThumbnailSize.Small);
        }

        /// <summary>
        /// Gets the thumbnail of an image given its path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Task<byte[]> GetThumbnail(string path, ThumbnailSize size)
        {
            return GetThumbnail(path, size, CancellationToken.None);
        }

        /// <summary>
        /// Gets the thumbnail of an image given its path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> GetThumbnail(string path, ThumbnailSize size, CancellationToken cancellationToken)
        {
            var request = MakeThumbnailRequest(path, size);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Gets a url for the thumbnail of an image (default size = small)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Uri GetThumbnailUrl(string path)
        {
            var request = MakeThumbnailRequest(path, ThumbnailSize.Small);

            return request.RequestUri;
        }

        /// <summary>
        /// Gets a url for the thumbnail of an image
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Uri GetThumbnailUrl(string path, ThumbnailSize size)
        {
            var request = MakeThumbnailRequest(path, size);

            return request.RequestUri;
        }

        /// <summary>
        /// Gets the deltas for a user's folders and files.
        /// </summary>
        /// <param name="cursor">The value returned from the prior call to GetDelta or an empty string</param>
        /// <returns></returns>
        public Task<DeltaPage> GetDelta(string cursor)
        {
            return GetDelta(cursor, CancellationToken.None);
        }

        /// <summary>
        /// Gets the deltas for a user's folders and files.
        /// </summary>
        /// <param name="cursor">The value returned from the prior call to GetDelta or an empty string</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeltaPage> GetDelta(string cursor, CancellationToken cancellationToken)
        {
            var requestUrl = MakeRequestString("1/delta", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Post, requestUrl);

            request.AddParameter("cursor", cursor);

            var deltaResponse = await SendAsync<DeltaPageInternal>(request, cancellationToken);

            var deltaPage = new DeltaPage
            {
                Cursor = deltaResponse.Cursor,
                Has_More = deltaResponse.Has_More,
                Reset = deltaResponse.Reset,
                Entries = new List<DeltaEntry>()
            };

            foreach (var stringList in deltaResponse.Entries)
            {
                deltaPage.Entries.Add(JRawListToDeltaEntry(stringList));
            }

            return deltaPage;
        }

        /// <summary>
        /// A long-poll endpoint to wait for changes on an account. In conjunction with /delta, this call gives you a low-latency way to monitor an account for file changes.
        /// </summary>
        /// <param name="cursor">The value returned from the prior call to GetDelta.</param>
        /// <param name="timeout">An optional integer indicating a timeout, in seconds.
        ///  The default value is 30 seconds, which is also the minimum allowed value. The maximum is 480 seconds.</param>
        /// <returns></returns>
        public Task<LongpollDeltaResult> GetLongpollDelta(string cursor, int timeout = 30)
        {
            return GetLongpollDelta(cursor, CancellationToken.None, timeout);
        }

        /// <summary>
        /// A long-poll endpoint to wait for changes on an account. In conjunction with /delta, this call gives you a low-latency way to monitor an account for file changes.
        /// </summary>
        /// <param name="cursor">The value returned from the prior call to GetDelta.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout">An optional integer indicating a timeout, in seconds.
        ///  The default value is 30 seconds, which is also the minimum allowed value. The maximum is 480 seconds.</param>
        /// <returns></returns>
        public async Task<LongpollDeltaResult> GetLongpollDelta(string cursor, CancellationToken cancellationToken, int timeout = 30)
        {
            var requestUrl = MakeRequestString("1/longpoll_delta", ApiType.Base);

            var request = new HttpRequest(HttpMethod.Get, requestUrl);

            request.AddParameter("cursor", cursor);

            if (timeout < 30)
                timeout = 30;
            if (timeout > 480)
                timeout = 480;
            request.AddParameter("timeout", timeout);

            return await SendAsync<LongpollDeltaResult>(request, cancellationToken);
        }
    }
}
