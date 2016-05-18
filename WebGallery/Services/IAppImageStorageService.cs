using System.IO;

namespace WebGallery.Services
{
    public interface IAppImageStorageService
    {
        void CreateContainerIfNotExists();
        string Upload(int submissionId, string imageName, Stream stream);
        void Delete(int submissionId, string imageName);
    }
}
