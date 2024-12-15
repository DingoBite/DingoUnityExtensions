using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyText : KeyTextReplacer
    {
        [SerializeField] protected TMP_Text Text;

        private void Reset()
        {
            Text ??= GetComponent<TMP_Text>();
            Template = Text.text;
        }

        protected override void ReplaceKeyBy(string text)
        {
            Text.text = ReplaceKeyBy(text, TemplateString);
        }
        
        public static string ReplaceKeyBy(string text, string template)
        {
            if (!Pattern.IsMatch(template))
            {
                return text;
            }

            var resultText = Pattern.Replace(template, text);
            return resultText;
        }
    }
}