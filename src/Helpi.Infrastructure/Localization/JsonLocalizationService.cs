using System.Globalization;
using System.Text.Json;
using Helpi.Application.Common.Interfaces;

namespace MyApp.Infrastructure.Localization
{
    public class JsonLocalizationService : ILocalizationService
    {
        private readonly string _defaultCulture;
        private readonly Dictionary<string, Dictionary<string, string>> _resources = new();

        public JsonLocalizationService(string defaultCulture = "en")
        {
            _defaultCulture = defaultCulture;
            LoadAll();
        }


        private void LoadAll()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "Localization/Langs");
            if (!Directory.Exists(folder)) return;

            foreach (var file in Directory.GetFiles(folder, "*.json"))
            {
                var culture = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);

                using var doc = JsonDocument.Parse(json);
                var dict = new Dictionary<string, string>();
                FlattenJson(doc.RootElement, dict);

                _resources[culture] = dict;
            }
        }

        private static void FlattenJson(JsonElement element, Dictionary<string, string> dict, string parentKey = "")
        {
            foreach (var prop in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(parentKey)
                    ? prop.Name
                    : $"{parentKey}.{prop.Name}";

                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    FlattenJson(prop.Value, dict, key);
                }
                else
                {
                    dict[key] = prop.Value.GetString() ?? string.Empty;
                }
            }
        }

        public string GetString(string key, string? culture = null, params object[] args)
        {
            var lang = (culture ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                .Split('-').First(); // handle fr-FR etc.

            if (_resources.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var value))
                return string.Format(value, args);

            if (_resources.TryGetValue(_defaultCulture, out var fallback) && fallback.TryGetValue(key, out var fallbackValue))
                return string.Format(fallbackValue, args);

            return $"[{key}]";
        }
    }
}
