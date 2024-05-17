using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class SingleKeyMultiTextReplacer : KeyTextReplacer
    {
        [SerializeField] private List<TMP_Text> _texts;

        protected override void ReplaceKeyBy(string text)
        {
            var resultText = Pattern.IsMatch(TemplateString) ? Pattern.Replace(TemplateString, text) : text;
            foreach (var t in _texts)
            {
                t.text = resultText;
            }
        }
    }
}