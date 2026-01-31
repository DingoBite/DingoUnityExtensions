using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DingoUnityExtensions.ImageLoadGlobalSystem
{
    public static class ImageLoadUtils
    {
        private static int _counter;
        private const string LOAD_DELAY = "LOAD_DELAY";

        public static void LoadNullUnload(this ImageLoadWrapper imageLoadWrapper, string url)
        {
            if (url == null)
                imageLoadWrapper.Unload();
            else 
                imageLoadWrapper.UpdateValueWithLoad(new ImageLoadHandle(url));
        }

        public static void UnloadAndCancelDelays(this ImageLoadWrapper imageLoadWrapper)
        {
            CoroutineParent.CancelCoroutine((imageLoadWrapper, LOAD_DELAY));
            imageLoadWrapper.Unload();
        }
        
        public static void LoadNullUnloadWithRandomDelay(this ImageLoadWrapper imageLoadWrapper, string url, float minDelay, float maxDelay)
        {
            if (url == null)
                return;
            // var delay = _counter++ % 2 * 1f;
            var delay = minDelay + Random.value * maxDelay;
            if (delay <= Vector3.kEpsilon)
            {
                imageLoadWrapper.UpdateValueWithLoad(new ImageLoadHandle(url, true));
            }
            else
            {
                imageLoadWrapper.Unload();
                CoroutineParent.InvokeAfterSecondsWithCanceling((imageLoadWrapper, LOAD_DELAY), delay, () => imageLoadWrapper.UpdateValueWithLoad(new ImageLoadHandle(url, true)));
            }
        }
    }
}