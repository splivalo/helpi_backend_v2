using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Helpi.Infrastructure.Configuration;

public static class FirebaseConfiguration
{
    public static void InitializeFirebase(IConfiguration configuration)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var credentialJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON")
                                    ??
                                    configuration["Firebase:CredentialsJson"];

            if (string.IsNullOrEmpty(credentialJson))
                throw new ArgumentNullException("Firebase credentials not configured");

            try
            {
                FirebaseApp.Create(new AppOptions
                {
                    // Credential = GoogleCredential.FromFile(serviceAccountPath)
                    Credential = GoogleCredential.FromJson(credentialJson)
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Firebase initialization failed", ex);
            }
        }
    }
}

