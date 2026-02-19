using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using SchoolManager.Services;

namespace SchoolManager.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly string _connectionString;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage")!;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeader = new BlobHttpHeaders { ContentType = file.ContentType };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });
            }

            return blobClient.Uri.ToString();
        }

        public string GetSecureUrl(string fileUrl, string originalName)
        {
            if (string.IsNullOrEmpty(fileUrl)) return "";

            Uri uri = new Uri(fileUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            string containerName = uri.Segments[1].Replace("/", "");

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (blobClient.CanGenerateSasUri)
            {
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(20),
                    ContentDisposition = $"attachment; filename=\"{originalName}\""
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }

            return fileUrl;
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            Uri uri = new Uri(fileUrl);
            string fileName = Path.GetFileName(uri.LocalPath);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}