using System.Configuration;
using System.IO;
using WebGallery.Utilities;

namespace WebGallery.Services
{
    public class AppImageAzureStorageService : IAppImageStorageService
    {
        public string ConnectionString { get; } = ConfigurationManager.AppSettings["AzureStorageConnectionString"];
        public string ContainerName { get; } = CONTAINER_NAME_LOGOS_SCREENSHOTS;

        public void CreateContainerIfNotExists()
        {
            AzureStorageHelper.CreateContainerIfNotExists(ConnectionString, ContainerName);
        }

        public string Upload(Stream stream, string appId, string imageName, int? submissionId)
        {
            return AzureStorageHelper.Upload(ConnectionString, ContainerName, GetBlobName(appId, imageName, submissionId), stream);
        }

        public void Delete(string appId, string imageName, int? submissionId)
        {
            AzureStorageHelper.Delete(ConnectionString, ContainerName, GetBlobName(appId, imageName, submissionId));
        }

        private static string GetBlobName(string appId, string imageName, int? submissionId)
        {
            return submissionId.HasValue ? $"{appId}-{imageName}-{submissionId.Value}" : $"{appId}-{imageName}";
        }

        // All logos and screenshots will be placed in this container.
        public const string CONTAINER_NAME_LOGOS_SCREENSHOTS = "logos-screenshots";
    }
}