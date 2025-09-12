using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.MaterialPropertiesAccess
{
    [Serializable, Preserve]
    public class MaterialPropertyAccess
    {
        protected int PropertyId = -1;
        protected string PropertyNameForId;
        public string PropertyName;

        public void SetValue<T>(MaterialPropertyBlock materialPropertyBlock, T value)
        {
            if (PropertyId < 0 || PropertyNameForId != PropertyName)
            {
                PropertyId = Shader.PropertyToID(PropertyName);
                PropertyNameForId = PropertyName;
            }

            materialPropertyBlock.SetValue(PropertyId, value);
        }

        public void SetValue<T>(Material material, T value)
        {
            if (PropertyId < 0 || PropertyNameForId != PropertyName)
            {
                PropertyId = Shader.PropertyToID(PropertyName);
                PropertyNameForId = PropertyName;
            }

            material.SetValue(PropertyId, value);
        }
    }
}