using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using static Sulakore.NativeMethods;

namespace Sulakore.Habbo.Messages
{
    public abstract class Identifiers
    {
        private readonly string _section;
        private readonly Dictionary<string, List<string>> _hashNames;

        public Identifiers()
        {
            _section = GetType().Name;
            _hashNames = new Dictionary<string, List<string>>();
        }

        public string[] GetHashName(string hash)
        {
            if (_hashNames.TryGetValue(hash, out List<string> names))
            {
                return names.ToArray();
            }
            return new string[0];
        }

        public void LoadFromHashes(HGame game, string path)
        {
            path = Path.GetFullPath(path);
            foreach (PropertyInfo property in GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var output = new char[64];
                GetPrivateProfileString(_section, property.Name, "-", output, output.Length, path);
                if (output[0] == '-')
                {
                    throw new KeyNotFoundException($"Failed to find key '{property.Name}' in '{path}'.");
                }

                var hash = new string(output).TrimEnd('\0');

                var id = ushort.MaxValue;
                if (game.Messages.TryGetValue(hash, out List<MessageItem> messages) && messages.Count == 1)
                {
                    id = messages.First().Id;
                }

                List<string> names = null;
                if (!_hashNames.TryGetValue(hash, out names))
                {
                    names = new List<string>();
                    _hashNames.Add(hash, names);
                }
                names.Add(property.Name);

                property.SetValue(this, id);
            }
        }
    }
}