using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Configuration;

public static class FirebaseConfiguration
{
    public static bool TryInitializeFirebase(IConfiguration configuration, ILogger logger, bool isDevelopment)
    {
        if (TryGetDefaultApp() != null)
        {
            return true;
        }

        var credentialPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON")
                           ?? configuration["Firebase:CredentialsJson"];

        logger.LogInformation("🔥 Credential path resolved to: {CredentialPath}", credentialPath);

        if (string.IsNullOrEmpty(credentialPath))
        {
            return HandleInitializationFailure(
                logger,
                isDevelopment,
                "⚠️ Firebase initialization skipped — credentials path is not configured.",
                new InvalidOperationException("FIREBASE_CREDENTIALS_JSON environment variable not set"));
        }

        if (!File.Exists(credentialPath))
        {
            return HandleInitializationFailure(
                logger,
                isDevelopment,
                $"⚠️ Firebase initialization skipped — credentials file not found: {credentialPath}",
                new FileNotFoundException($"Firebase credentials file not found: {credentialPath}"));
        }

        try
        {
            var json = File.ReadAllText(credentialPath);
            var credential = GoogleCredential.FromJson(json);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });

            logger.LogInformation("🔥 Firebase initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            return HandleInitializationFailure(
                logger,
                isDevelopment,
                "⚠️ Firebase initialization skipped — credentials are invalid for this environment.",
                new InvalidOperationException("[FAILED] Firebase initialization failed", ex));
        }
    }

    private static FirebaseApp? TryGetDefaultApp()
    {
        try
        {
            return FirebaseApp.DefaultInstance;
        }
        catch
        {
            return null;
        }
    }

    private static bool HandleInitializationFailure(
        ILogger logger,
        bool isDevelopment,
        string developmentMessage,
        Exception exception)
    {
        if (isDevelopment)
        {
            logger.LogWarning(developmentMessage);
            return false;
        }

        throw exception;
    }
}