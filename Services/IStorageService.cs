namespace SchoolManager.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName);
        Task DeleteFileAsync(string fileName, string containerName);
    }
}