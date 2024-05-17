using System.Text.RegularExpressions;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public abstract class KeyTextReplacer : ValueContainer<string>
    {
        protected static readonly Regex Pattern = new(@"\{[\w\s]+\}");
        
        [SerializeField] private string _notInteractablePlaceholder = "None";
        [FormerlySerializedAs("_templateString")] [SerializeField, TextArea] protected string Template;
        
        private string _value;
        public override string Value => _value;

        protected override string NonInteractablePlaceholder => _notInteractablePlaceholder;

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

        public override void SetValueWithoutNotify(string text)
        {
            _value = text;
            ReplaceKeyBy(text);
        }
        
        protected abstract void ReplaceKeyBy(string text);

        protected override void SubscribeOnly()
        {
        }

        protected override void UnsubscribeOnly()
        {
        }
    }
}