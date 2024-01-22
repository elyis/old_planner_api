namespace webApiTemplate.src.App.IService
{
    public interface IFileUploaderService
    {
        Task<string?> UploadFileAsync(string directoryPath, Stream stream, string fileExtension);
        Task<byte[]?> GetStreamFileAsync(string directoryPath, string filename);
    }
}