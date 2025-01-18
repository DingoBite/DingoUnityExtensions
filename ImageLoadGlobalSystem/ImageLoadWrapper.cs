using System;
using System.IO;
using Bind;
using DingoUnityExtensions.Generic;
using DingoUnityExtensions.SleepSystem;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Text;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public class ImageLoadWrapper : ValueContainer<ImageLoadHandle>, IContainer<UISleepable>
    {
        [SerializeField] private UISleepable _sleepable;
        [SerializeField] private RevealCanvasGroup _imageParent;
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private bool _autoManageLifetime = true;

        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        
        [SerializeField] private RevealCanvasGroup _preloader;
        [SerializeField] private RevealCanvasGroup _notFound;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField, ShowIf(nameof(IsDefaultLayoutSize))] private Vector2 _defaultLayoutElementSizes;
        [SerializeField] private string _nameTemplate = "{0}";

        private bool _isFirstValue;
        
        private bool IsDefaultLayoutSize => _layoutElement != null;
        public UISleepable ComponentElement => _sleepable;

        public void ApplyRawImageAction(Action<RawImage> action)
        {
            if (_rawImage == null)
                return;
            action?.Invoke(_rawImage);
        }

        public void UpdateValueWithLoad(ImageLoadHandle imageLoadHandle)
        {
            var isLoad = Value == null || Value != imageLoadHandle;
            if (isLoad && Value != null)
                Unload();
            UpdateValueWithoutNotify(imageLoadHandle);
            if (isLoad || imageLoadHandle.TextureFlow.V.State == ImageLoadState.None)
                SetupForLoad();
        }
        
        public void SetupForLoad()
        {
            if (_autoManageLifetime)
            {
                Debug.LogError($"Cannot manage autolifetime ImageLoadWrapper");
                return;
            }
            SetSleepState(false);
        }

        public void Unload()
        {
            if (_autoManageLifetime)
            {
                Debug.LogError($"Cannot manage autolifetime ImageLoadWrapper");
                return;
            }
            SetSleepState(true);
        }

        protected override void PreviousValueFree(ImageLoadHandle previousData)
        {
            if (previousData == null)
                return;
            previousData.TextureFlow.UnSubscribe(UpdateImage);
        }

        protected override void OnAwake() => SetDefaultValue();

        protected void SetDefaultValue()
        {
            if (!_isFirstValue)
            {
                UpdateImage(new TextureLoadData(null, ImageLoadState.Loading, ""));
                _isFirstValue = true;
            }
        }
        
        protected override void SetValueWithoutNotify(ImageLoadHandle value)
        {
            SetDefaultValue();
            
            name = "not found";
            if (value == null)
                return;

            name = SingleKeyText.ReplaceKeyBy(Path.GetFileNameWithoutExtension(value.Path), _nameTemplate);
            if (value.TextureFlow.V.State != ImageLoadState.None)
                UpdateImage(value.TextureFlow.V);
            value.TextureFlow.SafeSubscribeAndSet(UpdateImage);
            if (_autoManageLifetime && gameObject.activeInHierarchy && enabled && _sleepable == null)
                ImageGlobalLoadManager.Instance.Enable(this);
        }

        private void UpdateImage(TextureLoadData textureLoadData)
        {
            switch (textureLoadData.State)
            {
                case ImageLoadState.None:
                    setActive(_imageParent, false);
                    if (_preloader != null)
                        setActive(_preloader, true);
                    setActive(_notFound, false);
                    break;
                case ImageLoadState.Loading:
                    setActive(_imageParent, false);
                    if (_preloader != null)
                        setActive(_preloader, true);
                    setActive(_notFound, false);
                    break;
                case ImageLoadState.NotFound:
                    setActive(_imageParent, false);
                    if (_preloader != null)
                        setActive(_preloader, false);
                    setActive(_notFound, true);
                    break;
                case ImageLoadState.Loaded:
                    setActive(_imageParent, true);
                    if (_preloader != null)
                        setActive(_preloader, false);
                    setActive(_notFound, false);
                    var textureObj = textureLoadData.Texture;
                    var size = new Vector2();
                    if (textureObj is Texture texture)
                    {
                        _rawImage.texture = texture;
                        if (texture != null)
                        {
                            size.x = texture.width;
                            size.y = texture.height;
                        }
                    }

                    if (size.magnitude > 0)
                    {
                        var aspectRatio = size.x / size.y;

                        if (_aspectRatioFitter != null)
                        {
                            _aspectRatioFitter.aspectRatio = aspectRatio;
                        }

                        if (_layoutElement != null)
                        {
                            var defaultAspectRatio = _defaultLayoutElementSizes.x / _defaultLayoutElementSizes.y;
                            var scale = aspectRatio / defaultAspectRatio;
                            
                            _layoutElement.minWidth = _defaultLayoutElementSizes.x * scale;
                            _layoutElement.preferredWidth = _defaultLayoutElementSizes.x * scale;
                            
                            _layoutElement.minHeight = _defaultLayoutElementSizes.y * scale;
                            _layoutElement.preferredHeight = _defaultLayoutElementSizes.y * scale;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;

            void setActive(AnimatableBehaviour animatableBehaviour, bool value)
            {
                if (!gameObject.activeInHierarchy)
                    animatableBehaviour.SetActiveImmediately(value);
                else 
                    animatableBehaviour.AnimatableSetActive(value);
            }
        }
        
        private void SetSleepState(bool value)
        {
            if (ImageGlobalLoadManager.Instance == null)
                return;
            
            if (value)
                ImageGlobalLoadManager.Instance.Disable(this);
            else
                ImageGlobalLoadManager.Instance.Enable(this);
        }

        protected override void SubscribeOnly()
        {
            if (!_autoManageLifetime)
                return;
            
            if (_sleepable != null)
            {
                _sleepable.Sleeping.SafeSubscribe(SetSleepState);
                SetSleepState(_sleepable.Sleeping.V);
            }
            else if (ImageGlobalLoadManager.Instance != null)
            {
                ImageGlobalLoadManager.Instance.Enable(this);
            }
        }

        protected override void UnsubscribeOnly()
        {
            if (!_autoManageLifetime)
                return;
            
            if (_sleepable == null && ImageGlobalLoadManager.Instance != null)
                ImageGlobalLoadManager.Instance.Disable(this);
        }
    }
}