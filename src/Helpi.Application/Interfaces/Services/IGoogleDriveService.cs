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

    Task DeleteFileAsync(string fileIdentifier);

    Task<string> UploadFileToFolderAsync(
        string folderId,
        byte[] fileData,
        string fileName,
        string mimeType);

    Task<string?> FindFileInFolderAsync(string folderId, string fileName);
    Task<byte[]> DownloadFileAsync(string fileId);
    Task<string> UpdateFileAsync(string fileId, byte[] fileData, string mimeType);
}