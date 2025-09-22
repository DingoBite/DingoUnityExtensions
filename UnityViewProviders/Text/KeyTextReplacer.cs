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
        
        protected override string NonInteractablePlaceholder => _notInteractablePlaceholder;

        public string TemplateString
        {
            get => Template;
            set => Template = value;
        }

        public void RefreshValue()
        {
            SetValueWithoutNotify(Value);
        }

        protected override void SetValueWithoutNotify(string value) => ReplaceKeyBy(value);
        protected abstract void ReplaceKeyBy(string text);
    }
}