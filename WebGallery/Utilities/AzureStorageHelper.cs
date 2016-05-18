using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace WebGallery.Utilities
{
    public class AzureStorageHelper
    {
        public static void CreateContainerIfNotExists(string azureStorageConnectionString, string containerName)
        {
            var container = GetContainer(azureStorageConnectionString, containerName);

            // Create the container if it doesn't already exist
            container.CreateIfNotExists();

            // Enable blob-level public access
            container.SetPermissions(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public static string Upload(string azureStorageConnectionString, string containerName, string blobName, Stream source)
        {
            var container = GetContainer(azureStorageConnectionString, containerName);

            // Retrieve reference to a blob
            var blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a stream
            blockBlob.UploadFromStream(source);

            return blockBlob.Uri.ToString();
        }

        public static void Delete(string azureStorageConnectionString, string containerName, string blobName)
        {
            var container = GetContainer(azureStorageConnectionString, containerName);

            // Retrieve reference to a blob
            var blockBlob = container.GetBlockBlobReference(blobName);

            // Delete the blob
            blockBlob.Delete();
        }

        private static CloudBlobContainer GetContainer(string azureStorageConnectionString, string containerName)
        {
            // Retrieve storage account from connection string
            var storageAccount = CloudStorageAccount.Parse(azureStorageConnectionString);

            // Create the blob client
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container
            var container = blobClient.GetContainerReference(containerName);

            return container;
        }
    }
}