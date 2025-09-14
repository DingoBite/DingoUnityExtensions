using System;
using System.Collections.Generic;
using System.Linq;
using DingoUnityExtensions.UnityViewProviders.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace UnicodeFontIcons
{
    [Serializable, Preserve]
    public class UnicodeIcon
    {
        public string IconKey;
        public FontWeight FontWeight;
    }

    public class UnicodeTMP_Text : ValueContainer<UnicodeIcon>
    {
        [SerializeField] private TMP_Text _tmpText;
        [SerializeField] private UnicodeIcon _icon;
        
        [SerializeField] private string _fallbackIconKey;
        [SerializeField] private string _fallbackText;
        [SerializeField] private bool _caseSensitive;
        
        private IReadOnlyDictionary<string, string> _unicodeMapping;
        private string[] _keys;
        
        public Graphic Graphic => _tmpText;
        public bool CaseSensitive => _caseSensitive;
        
        protected override void SetValueWithoutNotify(UnicodeIcon value)
        {
            _icon = value;
            if (_tmpText == null)
                _tmpText = GetComponent<TMP_Text>();
            _tmpText.fontWeight = _icon.FontWeight;
            var key = _caseSensitive ? _icon.IconKey : _icon.IconKey.ToLower();
            _tmpText.text = GetUnicode(key);
        }

        protected override void Validate() => ForceSetValue(_icon);

        public string GetUnicode(string key)
        {
            GetUnicodeMapping();
            if (_unicodeMapping.TryGetValue(key, out var unicode))
                return unicode;
            if (_unicodeMapping.TryGetValue(_fallbackIconKey, out unicode))
                return unicode;
            return _fallbackText;
        }

        public TMP_FontAsset GetTMPFontAsset()
        {
            if (_tmpText == null)
                _tmpText = GetComponent<TMP_Text>();
            return _tmpText.font;
        }

        public string[] GetAllKeys()
        {
            GetUnicodeMapping();
            return _keys;
        }
        
        public IReadOnlyDictionary<string, string> GetUnicodeMapping()
        {
            if (_unicodeMapping == null || _unicodeMapping.Count == 0)
            {
                _unicodeMapping = UnicodeFontIconsConfig.CollectKeyToUnicodeMapping(_tmpText.font);
                _keys = _unicodeMapping.Keys.OrderBy(k => k).ToArray();
            }
            return _unicodeMapping;
        }
    }
}