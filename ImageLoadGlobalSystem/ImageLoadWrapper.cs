using System;
using System.IO;
using Bind;
using DingoUnityExtensions.Tweens;
using DingoUnityExtensions.UnityViewProviders.Core;
using DingoUnityExtensions.UnityViewProviders.Text;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public class ImageLoadWrapper : ValueContainer<ImageLoadHandle>
    {
        [SerializeField] private RevealCanvasGroup _imageParent;
        [SerializeField] private RawImage _rawImage;

        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        
        [SerializeField] private RevealCanvasGroup _preloader;
        [SerializeField] private RevealCanvasGroup _notFound;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField, ShowIf(nameof(IsDefaultLayoutSize))] private Vector2 _defaultLayoutElementSizes;
        [SerializeField] private string _nameTemplate = "{0}";

        [SerializeField] private bool _autoManageLifetime;
        
        [SerializeField] private UnityEvent<bool> _loadUnloadEvent;
        
        private RectTransform _rectTransform;
        private bool? _load;
        
        public RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>(); 
        private bool IsDefaultLayoutSize => _layoutElement != null;
        public Texture CurrentTexture => _rawImage.texture;
        
        public void ForceSetImage(Texture2D texture)
        {
            Unload();
            UpdateImage(new TextureLoadData(texture, ImageLoadState.Loaded, ""));
        }

        public void UpdateValueWithLoad(ImageLoadHandle imageLoadHandle)
        {
            UpdateValueWithoutNotify(imageLoadHandle);
            Load();
        }

        public void Load()
        {
            if (_load != null && _load.Value)
                return;
            _loadUnloadEvent?.Invoke(true);
            _load = true;
            Value?.LoadFor(this);
        }

        public void Unload()
        {
            if (_load != null && !_load.Value)
                return;
            
            _loadUnloadEvent?.Invoke(false);
            _load = false;
            UpdateImage(TextureLoadData.None);
            if (Value == null)
                return;
            Value.TextureFlow.UnSubscribe(UpdateImage);
            Value.UnloadFor(this);
        }

        protected override void PreviousValueFree(ImageLoadHandle previousData) => Unload();

        protected override void SetValueWithoutNotify(ImageLoadHandle value)
        {
            name = "not found";
            if (value == null)
                return;

            if (_autoManageLifetime && isActiveAndEnabled)
            {
                Unload();
                Load();
            }
            
            name = SingleKeyText.ReplaceKeyBy(Path.GetFileNameWithoutExtension(value.Path), _nameTemplate);
            Value.TextureFlow.SafeSubscribeAndSet(UpdateImage);
        }

        private void UpdateImage(TextureLoadData textureLoadData)
        {
            switch (textureLoadData.State)
            {
                case ImageLoadState.None:
                    setActive(_imageParent, false);
                    setActive(_preloader, true);
                    setActive(_notFound, false);
                    break;
                case ImageLoadState.Loading:
                    setActive(_imageParent, false);
                    setActive(_preloader, true);
                    setActive(_notFound, false);
                    break;
                case ImageLoadState.NotFound:
                    setActive(_imageParent, false);
                    setActive(_preloader, false);
                    setActive(_notFound, true);
                    break;
                case ImageLoadState.Loaded:
                    setActive(_imageParent, true);
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

                        if (_layoutElement != null && _defaultLayoutElementSizes.y != 0)
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
                if (animatableBehaviour == null)
                    return;
                if (!gameObject.activeInHierarchy)
                    animatableBehaviour.SetActiveImmediately(value);
                else 
                    animatableBehaviour.AnimatableSetActive(value);
            }
        }

        protected override void OnEnable()
        {
            if (_autoManageLifetime)
                Load();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (_autoManageLifetime)
                Unload();
            base.OnDisable();
        }
    }
}