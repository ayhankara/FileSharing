using Microsoft.Extensions.DependencyInjection;
using SecureFileStorage.Services;
using SecureFileStorage.Services.Interfaces;
using System;



namespace SecureFileStorage
{
    public class FileStorageServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public FileStorageServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileStorageService GetStorageService(string storageType)
        {
            switch (storageType)
            {
                case "AzureBlob":
                    return _serviceProvider.GetRequiredService<AzureBlobStorageService>();
                case "Local":
                    return _serviceProvider.GetRequiredService<LocalFileStorageService>();
                default:
                    throw new ArgumentException($"Unsupported storage type: {storageType}");
            }
        }
    }
}
