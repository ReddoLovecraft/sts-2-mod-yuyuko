using BaseLib.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TH_Yuyuko.Scripts.Main
{
    public abstract class YuyukoPowerModel : CustomPowerModel
    {
        private static readonly Lazy<IReadOnlyDictionary<Type, string>> _iconKeyByType = new(CreateIconKeyByType);

        public override string? CustomPackedIconPath => $"res://TH_Yuyuko/Artworks/Powers/{GetIconKey()}32.png";
        public override string? CustomBigIconPath => $"res://TH_Yuyuko/Artworks/Powers/{GetIconKey()}64.png";

        public virtual void Trigger()
        {
            Flash();
        }

        private string GetIconKey()
        {
            Type type = GetType();
            if (_iconKeyByType.Value.TryGetValue(type, out string key))
            {
                return key;
            }
            return GetUppercaseAbbrev(type.Name);
        }

        private static IReadOnlyDictionary<Type, string> CreateIconKeyByType()
        {
            Type baseType = typeof(YuyukoPowerModel);
            Type[] types;
            try
            {
                types = baseType.Assembly.GetTypes();
            }
            catch
            {
                types = Array.Empty<Type>();
            }

            IEnumerable<Type> powerTypes = types
                .Where(t => t != null && t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.ContainsGenericParameters);

            var sorted = powerTypes
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ThenBy(t => t.FullName ?? t.Name, StringComparer.Ordinal)
                .ToList();

            var countsByKey = new Dictionary<string, int>(StringComparer.Ordinal);
            var result = new Dictionary<Type, string>();

            foreach (Type t in sorted)
            {
                string baseKey = GetUppercaseAbbrev(t.Name);
                if (!countsByKey.TryGetValue(baseKey, out int count))
                {
                    countsByKey[baseKey] = 1;
                    result[t] = baseKey;
                }
                else
                {
                    count++;
                    countsByKey[baseKey] = count;
                    result[t] = baseKey + count.ToString();
                }
            }

            return result;
        }

        private static string GetUppercaseAbbrev(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "P";
            }

            var chars = new List<char>(capacity: Math.Min(name.Length, 8));
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c))
                {
                    chars.Add(c);
                }
            }

            return chars.Count > 0 ? new string(chars.ToArray()) : "P";
        }
    }
    
}
