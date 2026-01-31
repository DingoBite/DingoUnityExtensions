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
    /// <summary>
    /// Do not use UpdateValueWithoutNotify on this class
    /// </summary>
    [Obsolete("Use Texture2DLoadHandle")]
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
        private TextureLoadData? _lastTextureLoadData;
        private bool _destroyed;

        public RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>(); 
        private bool IsDefaultLayoutSize => _layoutElement != null;
        public Texture CurrentTexture => _rawImage.texture;
        
        public void ForceSetImage(Texture2D texture)
        {
            if (_destroyed)
                return;
            Unload();
            UpdateImage(new TextureLoadData(texture, ImageLoadState.Loaded, ""));
        }

        public void UpdateValueWithLoad(ImageLoadHandle imageLoadHandle)
        {
            if (_destroyed)
                return;
            Unload();
            SetValue(imageLoadHandle);
            if (Value.Path != null)
                LoadOnly();
        }

        public void LoadOnly()
        {
            if (_destroyed)
                return;
            if (_load != null && _load.Value)
                return;
            _loadUnloadEvent?.Invoke(true);
            _load = true;
            Value?.LoadFor(this);
        }

        public void Unload()
        {
            if (_destroyed)
                return;
            if (_load != null && !_load.Value)
                return;
            _loadUnloadEvent?.Invoke(false);
            _load = false;
            UpdateImage(TextureLoadData.None);
            if (Value?.Path != null)
            {
                Value.TextureFlow.UnSubscribe(UpdateImage);
                Value.UnloadFor(this);
            }
        }
        
        public void SetValue(ImageLoadHandle value)
        {
            if (_destroyed)
                return;
            if (Value == value)
                return;
            
            Value = value;
            name = "not found";
            if (value?.Path == null)
            {
                UpdateImage(TextureLoadData.NotFound);
                return;
            }
            
            name = SingleKeyText.ReplaceKeyBy(Path.GetFileNameWithoutExtension(value.Path), _nameTemplate);
            Value.TextureFlow.SafeSubscribe(UpdateImage);
            if (isActiveAndEnabled && _autoManageLifetime)
                LoadOnly();
        }

        private void UpdateImage(TextureLoadData textureLoadData)
        {
            if (_lastTextureLoadData != null && _lastTextureLoadData.Value.Texture == textureLoadData.Texture)
                return;
            
            _lastTextureLoadData = textureLoadData;
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

            void setActive(AnimatableBehaviour animatableBehaviour, bool value, bool isImmediately = false)
            {
                if (animatableBehaviour == null)
                    return;
                if (!gameObject.activeInHierarchy)
                    animatableBehaviour.SetActiveImmediately(value);
                else 
                    animatableBehaviour.SetActive(value, isImmediately);
            }
        }

        protected override void SetValueWithoutNotify(ImageLoadHandle value) => SetValue(value);

        protected override void OnEnable()
        {
            if (_autoManageLifetime)
                LoadOnly();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (_autoManageLifetime)
                Unload();
            base.OnDisable();
        }

        private void OnDestroy()
        {
            _destroyed = true;
            Unload();
        }
    }
}