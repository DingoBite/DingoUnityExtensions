using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyTextInputField : KeyTextReplacer
    {
        [SerializeField] protected TMP_InputField Text;

        private void Reset()
        {
            Text = GetComponent<TMP_InputField>();
            Template = Text.text;
        }

        protected override void ReplaceKeyBy(string text)
        {
            if (text == null)
                text = string.Empty;
            var resultText = text;
            if (Pattern.IsMatch(TemplateString))
                resultText = Pattern.Replace(TemplateString, text);
            Text.text = resultText;
        }

        protected override void SubscribeOnly() => Text.onValueChanged.AddListener(SetValueWithNotify);
        protected override void UnsubscribeOnly() => Text.onValueChanged.RemoveListener(SetValueWithNotify);
    }
}