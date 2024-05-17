using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyText : KeyTextReplacer
    {
        [FormerlySerializedAs("_tmpText")] [SerializeField] protected TMP_Text Text;

        private void Reset()
        {
            Text = GetComponent<TMP_Text>();
            Template = Text.text;
        }

        protected override void ReplaceKeyBy(string text)
        {
            var resultText = Pattern.Replace(TemplateString, text);
            Text.text = resultText;
        }
    }
}