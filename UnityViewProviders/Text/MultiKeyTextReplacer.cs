using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public abstract class MultiKeyTextReplacer : ValueContainer<IEnumerable<KeyValuePair<string, string>>>
    {
        [SerializeField, TextArea] protected string Template;
        
        public string TemplateString
        {
            get => Template;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !value.Contains('{') && !value.Contains('}'))
                    return;
                Template = value;
            }
        }

        protected override void SetValueWithoutNotify(IEnumerable<KeyValuePair<string, string>> ienumerable)
        {
            var str = TemplateString;
            foreach (var (key, value) in ienumerable)
            {
                str = ReplaceKeyBy(str, key, value);
            }

            SetText(str);
        }

        protected abstract string ReplaceKeyBy(string mainText, string key, string text);
        protected abstract string SetText(string mainText);
    }
}