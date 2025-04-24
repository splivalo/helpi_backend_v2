namespace Helpi.Application.Interfaces.Services;
public interface IGoogleDriveService
{
    Task<string> UploadContractAsync(
        string folderName,
        byte[] fileData,
        string fileName,
        string mimeType = "application/pdf");

    Task<bool> FolderExistsAsync(string folderName);
    Task<string> CreateFolderAsync(string folderName);
}