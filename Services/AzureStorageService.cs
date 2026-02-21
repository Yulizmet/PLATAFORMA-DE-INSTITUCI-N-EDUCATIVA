using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace SchoolManager.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly IConfiguration _configuration;

        public AzureStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString(string connectionName)
            => _configuration.GetConnectionString(connectionName) ?? throw new Exception($"Connection string {connectionName} not found.");

        public async Task<string> UploadFileAsync(IFormFile file, string containerName, string connectionName = "AzureStorageProcedures")
        {
            var blobServiceClient = new BlobServiceClient(GetConnectionString(connectionName));
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

        public string GetSecureUrl(string fileUrl, string originalName, string connectionName = "AzureStorageProcedures")
        {
            if (string.IsNullOrEmpty(fileUrl)) return "";

            Uri uri = new Uri(fileUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            string containerName = uri.Segments[1].Replace("/", "");

            var blobServiceClient = new BlobServiceClient(GetConnectionString(connectionName));
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

        public async Task DeleteFileAsync(string fileUrl, string containerName, string connectionName = "AzureStorageProcedures")
        {
            var blobServiceClient = new BlobServiceClient(GetConnectionString(connectionName));
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            Uri uri = new Uri(fileUrl);
            string fileName = Path.GetFileName(uri.LocalPath);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}