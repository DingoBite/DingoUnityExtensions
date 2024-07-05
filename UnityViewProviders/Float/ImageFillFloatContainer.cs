using System.Collections.Generic;
using DingoUnityExtensions.UnityViewProviders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.UnityViewProviders.Float
{
    public class ImageFillFloatContainer : ValueContainer<float>
    {
        [SerializeField] private List<Image> _images;

        protected override void SetValueWithoutNotify(float value)
        {
            foreach (var image in _images)
            {
                image.fillAmount = value;
            }
        }
    }
}