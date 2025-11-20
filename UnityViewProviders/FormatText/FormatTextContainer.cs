using System;
using DingoUnityExtensions.UnityViewProviders.Core;
using TMPro;
using UnityEngine;

namespace DingoUnityExtensions.UnityViewProviders.FormatText
{
    public class FormatTextContainer : ValueContainer<PreFormatText>
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _formatTemplate;
        
        private void Reset()
        {
            _text ??= GetComponent<TMP_Text>();
            _formatTemplate = _text.text;
        }

        protected override void SetValueWithoutNotify(PreFormatText value)
        {
            if (value.ArgsArray == null)
            {
                _text.text = value.Fallback;
                return;
            }
            try
            {
                _text.text = string.Format(_formatTemplate, value.ArgsArray);
            }
            catch (Exception e)
            {
                _text.text = value.Fallback;
                Debug.LogException(e);
            }
        }
    }
}