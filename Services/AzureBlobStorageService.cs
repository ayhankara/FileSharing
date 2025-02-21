using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using SecureFileStorage.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecureFileStorage.Services
{

    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value;
            _containerName = configuration.GetSection("AzureBlobStorage:ContainerName").Value;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, true);
            return fileName; // Blob adını döndür
        }

        public async Task<Stream> DownloadFileAsync(string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var downloadResult = await blobClient.DownloadAsync();
            return downloadResult.Value.Content;
        }

        public async Task DeleteFileAsync(string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteAsync();
        }

        public string GenerateUniqueFileName(string originalFileName)
        {
            string fileExtension = Path.GetExtension(originalFileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            return uniqueFileName;
        }
    }
}
