using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DingoUnityExtensions.MaterialPropertiesAccess
{
    public readonly struct CustomTypeMaterialHandle<T>
    {
        public readonly Action<Material, string, T> ByKeyMaterialSetter;
        public readonly Action<Material, int, T> ByIdMaterialSetter;
        public readonly Action<MaterialPropertyBlock, string, T> ByKeyBlockSetter;
        public readonly Action<MaterialPropertyBlock, int, T> ByIdBlockSetter;

        public CustomTypeMaterialHandle(Action<Material, string, T> byKeyMaterialSetter, Action<Material, int, T> byIdMaterialSetter, Action<MaterialPropertyBlock, string, T> byKeyBlockSetter, Action<MaterialPropertyBlock, int, T> byIdBlockSetter)
        {
            ByKeyMaterialSetter = byKeyMaterialSetter;
            ByIdMaterialSetter = byIdMaterialSetter;
            ByKeyBlockSetter = byKeyBlockSetter;
            ByIdBlockSetter = byIdBlockSetter;
        }
    }

    public static class MaterialPropertySetExtensions
    {
        private static class GenericContainer<T>
        {
            public static CustomTypeMaterialHandle<T> CustomTypeMaterialHandle;
        }

        public static void RegisterCustomMaterialHandle<T>(CustomTypeMaterialHandle<T> customTypeMaterialHandle)
        {
            GenericContainer<T>.CustomTypeMaterialHandle = customTypeMaterialHandle;
        }

        public static void SetValue<T>(this Material material, string propertyKey, T value)
        {
            if (!material)
                throw new ArgumentNullException(nameof(material));

            if (CheckForNullForTexture(material, propertyKey, value))
                return;

            switch (value)
            {
                case float f:
                    material.SetFloat(propertyKey, f);
                    return;
                case double d:
                    material.SetFloat(propertyKey, (float)d);
                    return;
                case int i:
                    material.SetInt(propertyKey, i);
                    return;
                case bool b:
                    material.SetInt(propertyKey, b ? 1 : 0);
                    return;

                case Color c:
                    material.SetColor(propertyKey, c);
                    return;
                case Color32 c32:
                    material.SetColor(propertyKey, c32);
                    return;

                case Vector4 v4:
                    material.SetVector(propertyKey, v4);
                    return;
                case Vector3 v3:
                    material.SetVector(propertyKey, new Vector4(v3.x, v3.y, v3.z, 0f));
                    return;
                case Vector2 v2:
                    material.SetVector(propertyKey, new Vector4(v2.x, v2.y, 0f, 0f));
                    return;
                case Vector3Int v3i:
                    material.SetVector(propertyKey, new Vector4(v3i.x, v3i.y, v3i.z, 0f));
                    return;
                case Vector2Int v2i:
                    material.SetVector(propertyKey, new Vector4(v2i.x, v2i.y, 0f, 0f));
                    return;

                case Matrix4x4 m:
                    material.SetMatrix(propertyKey, m);
                    return;

                case RenderTexture rt:
                    material.SetTexture(propertyKey, rt);
                    return;
                case Texture2D t2d:
                    material.SetTexture(propertyKey, t2d);
                    return;
                case Texture tex:
                    material.SetTexture(propertyKey, tex);
                    return;

                case ComputeBuffer cb:
                    material.SetBuffer(propertyKey, cb);
                    return;
            }

            HandleCustomType(material, propertyKey, value);
        }

        public static void SetValue<T>(this Material material, int propertyId, T value)
        {
            if (!material)
                throw new ArgumentNullException(nameof(material));

            if (CheckForNullForTexture(material, propertyId, value))
                return;

            switch (value)
            {
                case float f:
                    material.SetFloat(propertyId, f);
                    return;
                case double d:
                    material.SetFloat(propertyId, (float)d);
                    return;
                case int i:
                    material.SetInt(propertyId, i);
                    return;
                case bool b:
                    material.SetInt(propertyId, b ? 1 : 0);
                    return;

                case Color c:
                    material.SetColor(propertyId, c);
                    return;
                case Color32 c32:
                    material.SetColor(propertyId, (Color)c32);
                    return;

                case Vector4 v4:
                    material.SetVector(propertyId, v4);
                    return;
                case Vector3 v3:
                    material.SetVector(propertyId, new Vector4(v3.x, v3.y, v3.z, 0f));
                    return;
                case Vector2 v2:
                    material.SetVector(propertyId, new Vector4(v2.x, v2.y, 0f, 0f));
                    return;
                case Vector3Int v3i:
                    material.SetVector(propertyId, new Vector4(v3i.x, v3i.y, v3i.z, 0f));
                    return;
                case Vector2Int v2i:
                    material.SetVector(propertyId, new Vector4(v2i.x, v2i.y, 0f, 0f));
                    return;

                case Matrix4x4 m:
                    material.SetMatrix(propertyId, m);
                    return;

                case RenderTexture rt:
                    material.SetTexture(propertyId, rt);
                    return;
                case Texture2D t2d:
                    material.SetTexture(propertyId, t2d);
                    return;
                case Texture tex:
                    material.SetTexture(propertyId, tex);
                    return;

                case ComputeBuffer cb:
                    material.SetBuffer(propertyId, cb);
                    return;
            }

            HandleCustomType(material, propertyId, value);
        }

        public static void SetValue<T>(this MaterialPropertyBlock mpb, string propertyKey, T value)
        {
            if (mpb == null)
                throw new ArgumentNullException(nameof(mpb));

            if (CheckForNullForTexture(mpb, propertyKey, value))
                return;

            switch (value)
            {
                case float f:
                    mpb.SetFloat(propertyKey, f);
                    return;
                case double d:
                    mpb.SetFloat(propertyKey, (float)d);
                    return;
                case int i:
                    mpb.SetInt(propertyKey, i);
                    return;
                case bool b:
                    mpb.SetInt(propertyKey, b ? 1 : 0);
                    return;

                case Color c:
                    mpb.SetColor(propertyKey, c);
                    return;
                case Color32 c32:
                    mpb.SetColor(propertyKey, (Color)c32);
                    return;

                case Vector4 v4:
                    mpb.SetVector(propertyKey, v4);
                    return;
                case Vector3 v3:
                    mpb.SetVector(propertyKey, new Vector4(v3.x, v3.y, v3.z, 0f));
                    return;
                case Vector2 v2:
                    mpb.SetVector(propertyKey, new Vector4(v2.x, v2.y, 0f, 0f));
                    return;
                case Vector3Int v3i:
                    mpb.SetVector(propertyKey, new Vector4(v3i.x, v3i.y, v3i.z, 0f));
                    return;
                case Vector2Int v2i:
                    mpb.SetVector(propertyKey, new Vector4(v2i.x, v2i.y, 0f, 0f));
                    return;

                case Matrix4x4 m:
                    mpb.SetMatrix(propertyKey, m);
                    return;

                case RenderTexture rt:
                    mpb.SetTexture(propertyKey, rt);
                    return;
                case Texture2D t2d:
                    mpb.SetTexture(propertyKey, t2d);
                    return;
                case Texture tex:
                    mpb.SetTexture(propertyKey, tex);
                    return;

                case ComputeBuffer cb:
                    mpb.SetBuffer(propertyKey, cb);
                    return;
            }

            HandleCustomType(mpb, propertyKey, value);
        }

        public static void SetValue<T>(this MaterialPropertyBlock mpb, int propertyId, T value)
        {
            if (mpb == null)
                throw new ArgumentNullException(nameof(mpb));

            if (CheckForNullForTexture(mpb, propertyId, value))
                return;

            switch (value)
            {
                case float f:
                    mpb.SetFloat(propertyId, f);
                    return;
                case double d:
                    mpb.SetFloat(propertyId, (float)d);
                    return;
                case int i:
                    mpb.SetInt(propertyId, i);
                    return;
                case bool b:
                    mpb.SetInt(propertyId, b ? 1 : 0);
                    return;

                case Color c:
                    mpb.SetColor(propertyId, c);
                    return;
                case Color32 c32:
                    mpb.SetColor(propertyId, (Color)c32);
                    return;

                case Vector4 v4:
                    mpb.SetVector(propertyId, v4);
                    return;
                case Vector3 v3:
                    mpb.SetVector(propertyId, new Vector4(v3.x, v3.y, v3.z, 0f));
                    return;
                case Vector2 v2:
                    mpb.SetVector(propertyId, new Vector4(v2.x, v2.y, 0f, 0f));
                    return;
                case Vector3Int v3i:
                    mpb.SetVector(propertyId, new Vector4(v3i.x, v3i.y, v3i.z, 0f));
                    return;
                case Vector2Int v2i:
                    mpb.SetVector(propertyId, new Vector4(v2i.x, v2i.y, 0f, 0f));
                    return;

                case Matrix4x4 m:
                    mpb.SetMatrix(propertyId, m);
                    return;

                case RenderTexture rt:
                    mpb.SetTexture(propertyId, rt);
                    return;
                case Texture2D t2d:
                    mpb.SetTexture(propertyId, t2d);
                    return;
                case Texture tex:
                    mpb.SetTexture(propertyId, tex);
                    return;

                case ComputeBuffer cb:
                    mpb.SetBuffer(propertyId, cb);
                    return;
            }

            HandleCustomType(mpb, propertyId, value);
        }

        private static bool CheckForNullForTexture<T>(Material material, string propertyKey, T value)
        {
            if ((value is null || (value is Object uo && uo == null)) && (typeof(T) == typeof(RenderTexture) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture)))
            {
                material.SetTexture(propertyKey, null);
                return true;
            }

            return false;
        }

        private static bool CheckForNullForTexture<T>(Material material, int propertyId, T value)
        {
            if ((value is null || (value is Object uo && uo == null)) && (typeof(T) == typeof(RenderTexture) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture)))
            {
                material.SetTexture(propertyId, null);
                return true;
            }

            return false;
        }

        private static bool CheckForNullForTexture<T>(MaterialPropertyBlock mpb, string propertyKey, T value)
        {
            if ((value is null || (value is Object uo && uo == null)) && (typeof(T) == typeof(RenderTexture) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture)))
            {
                mpb.SetTexture(propertyKey, null);
                return true;
            }

            return false;
        }

        private static bool CheckForNullForTexture<T>(MaterialPropertyBlock mpb, int propertyId, T value)
        {
            if ((value is null || (value is Object uo && uo == null)) && (typeof(T) == typeof(RenderTexture) || typeof(T) == typeof(Texture) || typeof(T) == typeof(Texture)))
            {
                mpb.SetTexture(propertyId, null);
                return true;
            }

            return false;
        }

        private static void HandleCustomType<T>(Material material, string propertyKey, T value)
        {
            var customTypeMaterialHandle = GenericContainer<T>.CustomTypeMaterialHandle;
            if (customTypeMaterialHandle.ByKeyMaterialSetter != null)
            {
                try
                {
                    customTypeMaterialHandle.ByKeyMaterialSetter(material, propertyKey, value);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw new NotSupportedException($"Custom type {typeof(T).Name} not supported");
                }
            }

            throw new NotSupportedException($"Type {typeof(T).Name} not supported. Try to call {nameof(RegisterCustomMaterialHandle)} to handle custom type");
        }

        private static void HandleCustomType<T>(Material material, int propertyId, T value)
        {
            var customTypeMaterialHandle = GenericContainer<T>.CustomTypeMaterialHandle;
            if (customTypeMaterialHandle.ByIdMaterialSetter != null)
            {
                try
                {
                    customTypeMaterialHandle.ByIdMaterialSetter(material, propertyId, value);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw new NotSupportedException($"Custom type {typeof(T).Name} not supported");
                }
            }

            throw new NotSupportedException($"Type {typeof(T).Name} not supported. Try to call {nameof(RegisterCustomMaterialHandle)} to handle custom type");
        }

        private static void HandleCustomType<T>(MaterialPropertyBlock mpb, string propertyKey, T value)
        {
            var customTypeMaterialHandle = GenericContainer<T>.CustomTypeMaterialHandle;
            if (customTypeMaterialHandle.ByKeyBlockSetter != null)
            {
                try
                {
                    customTypeMaterialHandle.ByKeyBlockSetter(mpb, propertyKey, value);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw new NotSupportedException($"Custom type {typeof(T).Name} not supported");
                }
            }

            throw new NotSupportedException($"Type {typeof(T).Name} not supported. Try to call {nameof(RegisterCustomMaterialHandle)} to handle custom type");
        }

        private static void HandleCustomType<T>(MaterialPropertyBlock mpb, int propertyId, T value)
        {
            var customTypeMaterialHandle = GenericContainer<T>.CustomTypeMaterialHandle;
            if (customTypeMaterialHandle.ByIdBlockSetter != null)
            {
                try
                {
                    customTypeMaterialHandle.ByIdBlockSetter(mpb, propertyId, value);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw new NotSupportedException($"Custom type {typeof(T).Name} not supported");
                }
            }

            throw new NotSupportedException($"Type {typeof(T).Name} not supported. Try to call {nameof(RegisterCustomMaterialHandle)} to handle custom type");
        }
    }
}