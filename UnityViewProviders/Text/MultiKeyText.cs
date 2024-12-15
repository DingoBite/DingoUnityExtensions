using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class MultiKeyText : MultiKeyTextReplacer
    {
        [SerializeField] protected TMP_Text Text;

        private void Reset()
        {
            Text ??= GetComponent<TMP_Text>();
            Template = Text.text;
        }

        protected override string ReplaceKeyBy(string mainText, string key, string text)
        {
            return mainText.Replace($"{{{key}}}", text);
        }

        protected override string SetText(string mainText)
        {
            return Text.text = mainText;
        }
    }
}