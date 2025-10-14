using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


///EXAMPLES
///=================

// "Minimax": {
//   "ClientID": "abc",
//   "OAuth": {
//     "ClientId": "x",
//     "ClientSecret": "y"
//   }
// }

// "Minimax": {
//   "CredentialsJson": "/secrets/minimax.json"
// }

// var creds = CredentialLoader.Load(configuration, "Minimax");
// var clientId = creds.GetString("ClientID"); // or GetString("OAuth:ClientId")
// var username = creds.GetString("Username");
// var pw = creds.GetString("Password");
namespace Helpi.Infrastructure.Utilities
{
    public sealed class CredentialAccessor : IDisposable
    {
        private readonly JsonDocument _doc;
        internal CredentialAccessor(JsonDocument doc) => _doc = doc ?? throw new ArgumentNullException(nameof(doc));

        /// <summary>Get raw JsonElement for a nested path (dot or colon separated).</summary>
        public bool TryGetElement(string path, out JsonElement elem)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            var parts = path.Replace(':', '.').Split('.', StringSplitOptions.RemoveEmptyEntries);
            JsonElement current = _doc.RootElement;
            foreach (var p in parts)
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(p, out current))
                {
                    elem = default;
                    return false;
                }
            }
            elem = current;
            return true;
        }

        public string? GetString(string path, bool throwIfMissing = true)
        {
            if (TryGetElement(path, out var e))
            {
                if (e.ValueKind == JsonValueKind.String) return e.GetString();
                return e.ToString(); // fallback: serialize non-string primitive/object
            }
            if (throwIfMissing) throw new InvalidOperationException($"Credential property '{path}' not found.");
            return null;
        }

        public int GetInt(string path, bool throwIfMissing = true)
        {
            if (TryGetElement(path, out var e))
            {
                if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v)) return v;
                if (int.TryParse(e.ToString(), out var parsed)) return parsed;
                throw new InvalidOperationException($"Credential property '{path}' is not an int.");
            }
            if (throwIfMissing) throw new InvalidOperationException($"Credential property '{path}' not found.");
            return default;
        }

        public bool GetBool(string path, bool throwIfMissing = true)
        {
            if (TryGetElement(path, out var e))
            {
                if (e.ValueKind == JsonValueKind.True) return true;
                if (e.ValueKind == JsonValueKind.False) return false;
                if (bool.TryParse(e.ToString(), out var parsed)) return parsed;
                throw new InvalidOperationException($"Credential property '{path}' is not a boolean.");
            }
            if (throwIfMissing) throw new InvalidOperationException($"Credential property '{path}' not found.");
            return default;
        }

        /// <summary>Return raw JSON string for a nested element (object/array/value).</summary>
        public string GetRawJson(string path)
        {
            if (!TryGetElement(path, out var e)) throw new InvalidOperationException($"Credential property '{path}' not found.");
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            e.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public void Dispose() => _doc?.Dispose();
    }

    public static class CredentialLoader
    {
        // Cache keyed by configPathOrName so subsequent loads are fast
        private static readonly ConcurrentDictionary<string, Lazy<CredentialAccessor>> _cache = new();

        /// <summary>
        /// Load credentials for a named section. Example: "Minimax"
        /// It looks for (in order):
        /// 1) ENV var: MINIMAX_CREDENTIALS_JSON (uppercased {name}_CREDENTIALS_JSON)
        /// 2) IConfiguration["Minimax:CredentialsJson"] (either file path OR raw JSON)
        /// 3) IConfiguration["Minimax:CredentialsFile"] (explicit file path)
        /// 4) IConfiguration.GetSection("Minimax") - if that section already contains JSON-like nested config, it will be serialized and used.
        /// </summary>
        public static CredentialAccessor Load(IConfiguration configuration, string sectionName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException(nameof(sectionName));

            // Build a stable cache key
            var cacheKey = $"cred::{sectionName}";

            var accessorLazy = _cache.GetOrAdd(cacheKey, _ => new Lazy<CredentialAccessor>(() =>
            {
                // 1) env variable convention: MINIMAX_CREDENTIALS_JSON
                var envKey = $"{sectionName.ToUpperInvariant()}_CREDENTIALS_JSON";
                var envVal = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrWhiteSpace(envVal))
                {
                    return CreateFromJsonString(envVal, origin: envKey);
                }

                // 2) configuration keys
                var cfgCandidate = configuration[$"{sectionName}:CredentialsJson"];
                if (!string.IsNullOrWhiteSpace(cfgCandidate))
                {
                    // If it's a path to an existing file -> read file; otherwise treat as JSON content
                    if (File.Exists(cfgCandidate))
                        return CreateFromFile(cfgCandidate);
                    return CreateFromJsonString(cfgCandidate, origin: $"{sectionName}:CredentialsJson");
                }

                // 3) explicit file key
                var fileCandidate = configuration[$"{sectionName}:CredentialsFile"];
                if (!string.IsNullOrWhiteSpace(fileCandidate))
                {
                    if (!File.Exists(fileCandidate)) throw new FileNotFoundException($"Credentials file not found at {fileCandidate}");
                    return CreateFromFile(fileCandidate);
                }

                // 4) fallback: if the section exists as structured configuration, serialize it
                var section = configuration.GetSection(sectionName);
                if (section.Exists())
                {
                    // serialize the IConfigurationSection to JSON
                    var dict = new System.Collections.Generic.Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var child in section.GetChildren())
                    {
                        // if child has nested children, include as object by recursively building; using a simple helper:
                        dict[child.Key] = BuildValue(child);
                    }

                    var json = JsonSerializer.Serialize(dict);
                    return CreateFromJsonString(json, origin: $"configuration section:{sectionName}");
                }

                throw new InvalidOperationException($"No credentials found for '{sectionName}'. Looked for env var {envKey}, config keys {sectionName}:CredentialsJson or {sectionName}:CredentialsFile, or a configuration section.");
            }));

            return accessorLazy.Value;
        }

        private static object? BuildValue(IConfigurationSection section)
        {
            if (!section.GetChildren().Any())
            {
                return section.Value;
            }

            var dict = new System.Collections.Generic.Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var child in section.GetChildren())
                dict[child.Key] = BuildValue(child);
            return dict;
        }

        private static CredentialAccessor CreateFromFile(string path)
        {
            var json = File.ReadAllText(path);
            return CreateFromJsonString(json, origin: path);
        }

        private static CredentialAccessor CreateFromJsonString(string json, string origin)
        {
            try
            {
                var doc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true });
                return new CredentialAccessor(doc);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse JSON credentials from {origin}: {ex.Message}", ex);
            }
        }

        /// <summary>Clear cache (useful for tests or reload scenarios).</summary>
        public static void ClearCache(string? sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                _cache.Clear();
                return;
            }

            var key = $"cred::{sectionName}";
            _cache.TryRemove(key, out _);
        }
    }
}
