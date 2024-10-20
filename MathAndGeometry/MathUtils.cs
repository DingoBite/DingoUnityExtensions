using System;
using UnityEngine;

namespace DingoUnityExtensions.MathAndGeometry
{
    public static class MathUtils
    {
        public static Matrix4x4 Lerp(in Matrix4x4 from, in Matrix4x4 to, float t)
        {
            var result = new Matrix4x4();

            for (var i = 0; i < 16; i++)
            {
                result[i] = Mathf.LerpUnclamped(from[i], to[i], t);
            }

            return result;
        }

        public static float SafeDivide(in float a, in float b)
        {
            if (Math.Abs(b) < float.Epsilon)
                return 0;
            return a / b;
        }
    }
}