using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mute.Utilities
{
    internal class IniParser
    {
        private readonly Dictionary<string, string> _values;

        public IniParser(string path)
        {
            if (File.Exists(path))
            {
                _values = File.ReadLines(path)
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .Select(line => line.Split(new[] { '=' }, 2))
                    .ToDictionary(parts => parts[0].Trim(), parts => parts.Length > 1 ? parts[1].Trim() : null);
            }
            else
                _values = new();
        }

        public string Value(string key, string defaultValue = null)
            => _values.TryGetValue(key, out var value) ? value : defaultValue;

        public string this[string key]
            => Value(key);

        public T Parse<T>(string key)
            => JsonConvert.DeserializeObject<T>(this[key]);
    }
}
