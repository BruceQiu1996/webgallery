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

        public string Upload(int submissionId, string imageName, Stream stream)
        {
            return AzureStorageHelper.Upload(ConnectionString, ContainerName, GetBlobName(submissionId, imageName), stream);
        }

        public void Delete(int submissionId, string imageName)
        {
            AzureStorageHelper.Delete(ConnectionString, ContainerName, GetBlobName(submissionId, imageName));
        }

        private static string GetBlobName(int submissionId, string imageName)
        {
            return $"{submissionId}-{imageName}";
        }

        // All logos and screenshots will be placed in this container.
        public const string CONTAINER_NAME_LOGOS_SCREENSHOTS = "logos-screenshots";
    }
}