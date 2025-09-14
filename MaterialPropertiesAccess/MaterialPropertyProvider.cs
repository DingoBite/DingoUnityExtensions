using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.MaterialPropertiesAccess
{
    [Serializable, Preserve]
    public class ListMaterialPropertyProvider
    {
        [SerializeField] private List<MaterialPropertyProvider> _providers;

        public void SetValue<T>(Renderer renderer, T value)
        {
            foreach (var materialPropertyProvider in _providers)
            {
                materialPropertyProvider.SetValue(renderer, value);
            }
        }
    }
    
    [Serializable, Preserve]
    public class MaterialPropertyProvider
    {
        protected int PropertyId = -1;
        protected string PropertyNameForId;
        public string PropertyName;

        private MaterialPropertyBlock _materialPropertyBlock;

        public void SetValue<T>(Renderer renderer, T value)
        {
            Setup(renderer);
            _materialPropertyBlock.SetValue(PropertyId, value);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        private void Setup(Renderer renderer)
        {
            if (PropertyId < 0 || PropertyNameForId != PropertyName)
            {
                PropertyId = Shader.PropertyToID(PropertyName);
                PropertyNameForId = PropertyName;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_materialPropertyBlock);
        }
    }
}