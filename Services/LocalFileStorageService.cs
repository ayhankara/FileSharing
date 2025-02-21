using Microsoft.Extensions.Configuration;
using SecureFileStorage.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
namespace SecureFileStorage.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storagePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storagePath = configuration.GetSection("LocalFileStorage:StoragePath").Value;
            Directory.CreateDirectory(_storagePath); // Klasörü oluştur, yoksa
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(file);
            }
            return fileName; // Dosya adını döndür
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (!File.Exists(filePath))
                return null;

            return File.OpenRead(filePath);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public string GenerateUniqueFileName(string originalFileName)
        {
            string fileExtension = Path.GetExtension(originalFileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            return uniqueFileName;
        }
    }
}
