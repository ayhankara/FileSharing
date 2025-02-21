using System.IO;
using System.Threading.Tasks;
namespace SecureFileStorage.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<Stream> DownloadFileAsync(string blobName);
        Task DeleteFileAsync(string blobName);
        string GenerateUniqueFileName(string originalFileName);
    }
}
