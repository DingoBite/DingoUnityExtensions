using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.Text
{
    public class ByPassTextReplacer : KeyTextReplacer
    {
        [SerializeField] private List<KeyTextReplacer> _textReplacers;
        
        protected override void ReplaceKeyBy(string text)
        {
            foreach (var t in _textReplacers)
            {
                t.UpdateValueWithoutNotify(text);
            }
        }
    }
}