using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.MaterialPropertyBlockAnimations
{
    [Serializable, Preserve]
    public abstract class MaterialPropertyBlockModifier
    {
        protected int PropertyId = -1;
        protected string PropertyNameForId;
        public string PropertyName;

        public void Apply(MaterialPropertyBlock materialPropertyBlock, float time)
        {
            if (PropertyId < 0 || PropertyNameForId != PropertyName)
            {
                PropertyId = Shader.PropertyToID(PropertyName);
                PropertyNameForId = PropertyName;
            }
            SetValue(materialPropertyBlock, time);
        }

        public void Apply(Material material, float time)
        {
            if (PropertyId < 0 || PropertyNameForId != PropertyName)
            {
                PropertyId = Shader.PropertyToID(PropertyName);
                PropertyNameForId = PropertyName;
            }
            SetValue(material, time);
        }

        protected abstract void SetValue(MaterialPropertyBlock materialPropertyBlock, float time);
        protected abstract void SetValue(Material material, float time);
    }
    
    [Serializable, Preserve]
    public class FloatMaterialPropertyBlockModifier : MaterialPropertyBlockModifier
    {
        public float DefaultValue;
        public float Min;
        public float Max;

        protected override void SetValue(MaterialPropertyBlock materialPropertyBlock, float time)
        {
            var value = time < 0 ? DefaultValue : Mathf.Lerp(Min, Max, time);
            materialPropertyBlock.SetFloat(PropertyId, value);
        }

        protected override void SetValue(Material material, float time)
        {
            var value = time < 0 ? DefaultValue : Mathf.Lerp(Min, Max, time);
            material.SetFloat(PropertyId, value);
        }
    }
    
    [Serializable, Preserve]
    public class IntMaterialPropertyBlockModifier : MaterialPropertyBlockModifier
    {
        public int DefaultValue;
        public int Min;
        public int Max;

        protected override void SetValue(MaterialPropertyBlock materialPropertyBlock, float time)
        {
            var value = time < 0 ? DefaultValue : (int)Math.Round(Mathf.Lerp(Min, Max, time));
            materialPropertyBlock.SetInt(PropertyId, value);
        }
        
        protected override void SetValue(Material material, float time)
        {
            var value = time < 0 ? DefaultValue : (int)Math.Round(Mathf.Lerp(Min, Max, time));
            material.SetInt(PropertyId, value);
        }
    }
    
    [Serializable, Preserve]
    public class ColorMaterialPropertyBlockModifier : MaterialPropertyBlockModifier
    {
        public Color DefaultValue;
        public Color Min;
        public Color Max;

        protected override void SetValue(MaterialPropertyBlock materialPropertyBlock, float time)
        {
            var value = time < 0 ? DefaultValue : Color.Lerp(Min, Max, time);
            materialPropertyBlock.SetColor(PropertyId, value);
        }
        
        protected override void SetValue(Material material, float time)
        {
            var value = time < 0 ? DefaultValue : Color.Lerp(Min, Max, time);
            material.SetColor(PropertyId, value);
        }
    }
    
    [Serializable, Preserve]
    public class Vector4MaterialPropertyBlockModifier : MaterialPropertyBlockModifier
    {
        public Vector4 DefaultValue;
        public Vector4 Min;
        public Vector4 Max;

        protected override void SetValue(MaterialPropertyBlock materialPropertyBlock, float time)
        {
            var value = time < 0 ? DefaultValue : Vector4.Lerp(Min, Max, time);
            materialPropertyBlock.SetVector(PropertyId, value);
        }
        
        protected override void SetValue(Material material, float time)
        {
            var value = time < 0 ? DefaultValue : Vector4.Lerp(Min, Max, time);
            material.SetVector(PropertyId, value);
        }
    }
}