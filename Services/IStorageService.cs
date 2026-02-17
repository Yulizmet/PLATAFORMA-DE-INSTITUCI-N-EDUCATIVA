namespace SchoolManager.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName, string connectionName = "AzureStorageProcedures");
        Task DeleteFileAsync(string fileUrl, string containerName, string connectionName = "AzureStorageProcedures");
        string GetSecureUrl(string fileUrl, string originalName, string connectionName = "AzureStorageProcedures");
    }
}