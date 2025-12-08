using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DingoUnityExtensions.MonoBehaviours.Singletons;
using NaughtyAttributes;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.UnicodeFontIcons
{
    [CreateAssetMenu(menuName = nameof(UnicodeFontIconsConfig), fileName = "S_" + nameof(UnicodeFontIconsConfig), order = 0)]
    public class UnicodeFontIconsConfig : ProtectedSingletonScriptableObject<UnicodeFontIconsConfig>
    {
        [Serializable, Preserve]
        private class FontIconDescriptor
        {
            public string unicode;
        }
        
        private static readonly Dictionary<string, string> Empty = new ();

        [SerializeField] private SerializedDictionary<TMP_FontAsset, string> _mappingAssetsByFont;

        private readonly Dictionary<TMP_FontAsset, Dictionary<string, string>> _mappingByFont = new();
        
        public static IReadOnlyDictionary<string, string> CollectKeyToUnicodeMapping(TMP_FontAsset fontAsset)
        {
            if (Instance == null)
                return Empty;
            if (Instance._mappingByFont.TryGetValue(fontAsset, out var mapping))
                return mapping;
            var resourcePath = Instance._mappingAssetsByFont.GetValueOrDefault(fontAsset);
            var json = Resources.Load<TextAsset>(resourcePath);
            if (json == null)
                return Empty;
            var icons = JsonConvert.DeserializeObject<Dictionary<string, FontIconDescriptor>>(json.text);
            if (icons == null)
                return Empty;
            mapping = new Dictionary<string, string>();
            foreach (var icon in icons)
            {
                mapping[icon.Key] = char.ConvertFromUtf32(int.Parse(icon.Value.unicode, System.Globalization.NumberStyles.HexNumber));
            }

            Instance._mappingByFont[fontAsset] = mapping;
            return mapping;
        }

#if VINSPECTOR_EXISTS
        [VInspector.Button]
#else
        [NaughtyAttributes.Button]
#endif
        private void ResetCache() => _mappingByFont.Clear();
    }
}