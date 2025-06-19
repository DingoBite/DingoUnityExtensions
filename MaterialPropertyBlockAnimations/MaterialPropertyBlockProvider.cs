using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.MaterialPropertyBlockAnimations
{
    [Serializable, Preserve]
    public class MaterialPropertyBlockProvider
    {
        protected int PropertyId = -1;
        protected string PropertyNameForId;
        public string PropertyName;

        private MaterialPropertyBlock _materialPropertyBlock;
        
        public void SetInt(Renderer renderer, int value)
        {
            Setup(renderer);
            _materialPropertyBlock.SetInt(PropertyId, value);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        public void SetFloat(Renderer renderer, float value)
        {
            Setup(renderer);
            _materialPropertyBlock.SetFloat(PropertyId, value);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        public void SetColor(Renderer renderer, Color color)
        {
            Setup(renderer);
            _materialPropertyBlock.SetColor(PropertyId, color);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        
        public void SetVector(Renderer renderer, Vector4 vector)
        {
            Setup(renderer);
            _materialPropertyBlock.SetVector(PropertyId, vector);
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