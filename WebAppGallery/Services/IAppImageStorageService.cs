using System.IO;

namespace WebGallery.Services
{
    public interface IAppImageStorageService
    {
        void CreateContainerIfNotExists();
        string Upload(Stream stream, string appId, string imageName, int? submissionId);
        void Delete(string appId, string imageName, int? submissionId);
    }
}