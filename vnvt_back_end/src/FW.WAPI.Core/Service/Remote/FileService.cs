using FW.WAPI.Core.DAL.Model.File;
using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.General;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Service.Remote
{
    public class FileService : IFileService
    {
        public readonly HttpClient _httpClient;
        public readonly IConfiguration _configuration;
        public readonly string urlImageUpload;
        public readonly string urlFileUpload;

        public FileService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            //urlImageUpload = _configuration.GetSection("imageFolder").Value;
            //urlImageUpload = _configuration.GetSection("fileFolder").Value;
        }

        /// <summary>
        /// Call to FileService to delete file or image
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path">path name of file or images</param>
        /// <returns>true/false</returns>
        /// <example></example>
        public virtual async Task<bool> Remove(string url, string path)
        {
            var fullPath = $"{url}/{path}";
            var result = await _httpClient.DeleteAsync(fullPath);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual async Task<FileInfoExt> UploadFile(string url, FileInfoExt file)
        {
            var multipart = new MultipartFormDataContent();
            multipart.Add(new ByteArrayContent(file.Content), "file", file.Name);
            var result = await _httpClient.PostAsync(url, multipart);

            if (result.IsSuccessStatusCode)
            {
                var stringResult = await result.Content.ReadAsStringAsync();
                var fileReuslt = JsonUtilities.ConvertJsonToObject<FileInfoExt>(stringResult);
                return fileReuslt;
            }
            else
            {
                throw new RemoteServiceException(result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual async Task<string> UploadFiles(string url, List<FileInfoExt> files)
        {
            var multipart = new MultipartFormDataContent();

            foreach (var file in files)
            {
                multipart.Add(new ByteArrayContent(file.Content), "file", file.Name);
            }

            var result = await _httpClient.PostAsync(url, multipart);

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }
            else
            {
                throw new RemoteServiceException(result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public virtual async Task<FileInfoExt> UploadImage(string url, FileInfoExt image)
        {

            var multipart = new MultipartFormDataContent();
            multipart.Add(new ByteArrayContent(image.Content), "file", image.Name);
            var result = await _httpClient.PostAsync(url, multipart);

            if (result.IsSuccessStatusCode)
            {
                var stringResult = await result.Content.ReadAsStringAsync();
                var fileReuslt = JsonUtilities.ConvertJsonToObject<FileInfoExt>(stringResult);
                return fileReuslt;
            }
            else
            {
                throw new RemoteServiceException(result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        public virtual async Task<string> UploadImages(string url, List<FileInfoExt> images)
        {
            var multipart = new MultipartFormDataContent();

            foreach (var image in images)
            {
                multipart.Add(new ByteArrayContent(image.Content), "file", image.Name);
            }

            var result = await _httpClient.PostAsync(url, multipart);

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }
            else
            {
                throw new RemoteServiceException(result.ReasonPhrase, (int)result.StatusCode);
            }
        }

        /// <summary>
        /// Get File
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public virtual async Task<Stream> GetFile(string fileUrl)
        {
            var result = await _httpClient.GetAsync(fileUrl);

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStreamAsync();
            }
            else
            {
                throw new RemoteServiceException(result.ReasonPhrase, (int)result.StatusCode);
            }
        }
    }

    public interface IFileService
    {
        Task<string> UploadFiles(string url, List<FileInfoExt> files);
        Task<FileInfoExt> UploadFile(string url, FileInfoExt file);
        Task<string> UploadImages(string url, List<FileInfoExt> images);
        Task<FileInfoExt> UploadImage(string url, FileInfoExt image);

        Task<bool> Remove(string url, string path);

        Task<Stream> GetFile(string fileUrl);

    }
}
