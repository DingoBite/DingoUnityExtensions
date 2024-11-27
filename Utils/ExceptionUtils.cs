using System;
using UnityEngine;

namespace DingoUnityExtensions.Utils
{
    public static class ExceptionUtils
    {
        public static void Throw<T>(this T exception, bool isSafe) where T : Exception
        {
            if (isSafe)
                Debug.LogException(exception);
            throw exception;
        }
    }
}