using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Helpi.Infrastructure.Configuration;

public static class FirebaseConfiguration
{
    public static void InitializeFirebase(IConfiguration configuration, ILogger logger)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialPath = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON")
                               ?? configuration["Firebase:CredentialsJson"];

            logger.LogInformation("🔥 Credential path resolved to: {CredentialPath}", credentialPath);

            if (string.IsNullOrEmpty(credentialPath))
            {
                logger.LogInformation("🔥 FIREBASE_CREDENTIALS_JSON environment variable not set");
                throw new InvalidOperationException("FIREBASE_CREDENTIALS_JSON environment variable not set");
            }

            if (!File.Exists(credentialPath))
            {
                logger.LogInformation("🔥 Firebase credentials file not found: {CredentialPath}", credentialPath);
                throw new FileNotFoundException($"Firebase credentials file not found: {credentialPath}");
            }

            try
            {
                var firstLine = File.ReadLines(credentialPath).First();

                var json = File.ReadAllText(credentialPath);
                var credential = GoogleCredential.FromJson(json);

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });

                logger.LogInformation("🔥 Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "🔥 [FAILED] Firebase initialization failed");
                throw new InvalidOperationException("[FAILED] Firebase initialization failed", ex);
            }
        }
    }
}