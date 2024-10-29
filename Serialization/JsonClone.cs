using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DingoUnityExtensions.Serialization
{
    public static class JsonClone
    {
        public static T CloneSerializable<T>(T o, JsonSerializerSettings settings = null)
        {
            if (Equals(o, default(T)))
                return default;
            try
            {
                var json = JsonConvert.SerializeObject(o, settings);
                var res = JsonConvert.DeserializeObject<T>(json, settings);
                return res;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
    }
}