using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly GoogleDriveSettings _settings;
    private readonly ILogger<GoogleDriveService> _logger;

    public GoogleDriveService(
        IOptions<GoogleDriveSettings> settings,
        ILogger<GoogleDriveService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }


    public async Task<string> UploadContractAsync(
        string folderName,
        byte[] fileData,
        string fileName,
        string mimeType = "application/pdf")
    {
        try
        {
            using var driveService = await CreateDriveServiceAsync();

            // Check/Create folder
            var folderId = await GetOrCreateFolderAsync(driveService, folderName);

            // File metadata
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { folderId },
                MimeType = mimeType
            };

            // Create request
            var request = driveService.Files.Create(
                fileMetadata,
                new MemoryStream(fileData),
                mimeType);
            request.Fields = "id, webViewLink";

            // Upload file
            var result = await request.UploadAsync();
            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new GoogleDriveException($"File upload failed: {result.Exception.Message}");

            // Set permissions to make accessible via link
            // var permission = new Google.Apis.Drive.v3.Data.Permission
            // {
            //     Type = "anyone",
            //     Role = "reader"
            // };
            // await driveService.Permissions.Create(permission, request.ResponseBody.Id)
            //     .ExecuteAsync();

            return request.ResponseBody.WebViewLink;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Drive upload failed");
            throw new GoogleDriveException("Failed to upload contract to Google Drive", ex);
        }
    }

    public async Task<bool> FolderExistsAsync(string folderName)
    {
        using var driveService = await CreateDriveServiceAsync();

        var request = driveService.Files.List();
        request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
        request.Fields = "files(id)";

        var result = await request.ExecuteAsync();
        return result.Files.Any();
    }

    public async Task<string> CreateFolderAsync(string folderName)
    {
        using var driveService = await CreateDriveServiceAsync();

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        };

        bool parentIsValid = false;

        if (!string.IsNullOrEmpty(_settings.BaseFolderId))
        {
            try
            {
                var checkRequest = driveService.Files.Get(_settings.BaseFolderId);
                checkRequest.Fields = "id";
                await checkRequest.ExecuteAsync();
                fileMetadata.Parents = new List<string> { _settings.BaseFolderId };
                parentIsValid = true;
            }
            catch
            {
                Console.WriteLine($"Warning: BaseFolderId '{_settings.BaseFolderId}' not valid. Defaulting to root.");
            }
        }

        if (!parentIsValid)
        {
            fileMetadata.Parents = new List<string> { "root" }; // ensures visibility in 'My Drive'
        }

        var request = driveService.Files.Create(fileMetadata);
        request.Fields = "id, owners(emailAddress)";
        var folder = await request.ExecuteAsync();

        Console.WriteLine($"Folder created. ID: {folder.Id}");

        /// give persmission to helpi email user
        /// because service user account is also a root user
        try
        {
            var permission = new Permission
            {
                Type = "user",
                Role = "writer",
                EmailAddress = "helpi.systems@gmail.com"
            };

            await driveService.Permissions.Create(permission, folder.Id).ExecuteAsync();

        }
        catch (Exception ex)
        {

            throw;
        }
        try
        {
            foreach (var owner in folder.Owners)
            {
                Console.WriteLine($"Folder owner: {owner.EmailAddress}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"---> {ex}");
            throw;
        }



        return folder.Id;
    }



    private async Task<DriveService> CreateDriveServiceAsync()
    {
        var credential = GoogleCredential.FromJson(_settings.CredentialsJson)
           .CreateScoped(DriveService.Scope.Drive);

        return new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _settings.ApplicationName
        });
    }



    private async Task<string> GetOrCreateFolderAsync(DriveService driveService, string folderName)
    {
        try
        {
            // Check if folder exists in base folder
            var listRequest = driveService.Files.List();
            listRequest.Q = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder' " +
                           $"and '{_settings.BaseFolderId}' in parents and trashed=false";
            listRequest.Fields = "files(id)";

            var existingFolders = await listRequest.ExecuteAsync();
            if (existingFolders.Files.Count > 0)
            {
                return existingFolders.Files[0].Id;
            }
        }
        catch (GoogleApiException ex)
        {
            Console.WriteLine($"Error during folder search: {ex.Message}");
            Console.WriteLine($"BaseFolderId used: {_settings.BaseFolderId}");
            throw; // Rethrow to keep the stack trace
        }

        // Create new folder if it doesn't exist
        return await CreateFolderAsync(folderName);
    }
}

public class GoogleDriveSettings
{
    public string ApplicationName { get; set; } = null!;
    public string BaseFolderId { get; set; } = null!;
    public string CredentialsJson { get; set; } = null!;
}

public class GoogleDriveException : Exception
{
    public GoogleDriveException(string message) : base(message) { }
    public GoogleDriveException(string message, Exception inner) : base(message, inner) { }
}