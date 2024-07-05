using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyText : KeyTextReplacer
    {
        [SerializeField] protected TMP_Text Text;

        private void Reset()
        {
            Text = GetComponent<TMP_Text>();
            Template = Text.text;
        }

        protected override void ReplaceKeyBy(string text)
        {
            if (!Pattern.IsMatch(TemplateString))
            {
                Text.text = text;
            }
            else
            {
                var resultText = Pattern.Replace(TemplateString, text);
                Text.text = resultText;
            }
        }
    }
}