﻿using DingoUnityExtensions.MonoBehaviours.Singletons;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours
{
    public class ScreenConfigure : ProtectedSingletonBehaviour<ScreenConfigure>
    {
        [SerializeField] private int _frameRate = 90;
        [SerializeField] private bool _everyFrameUpdate;

        public static void SetFrameRate(int framerate) => Instance._frameRate = framerate;
        
        private void Start()
        {
            Application.targetFrameRate = _frameRate;

            if (_everyFrameUpdate)
                CoroutineParent.AddUpdater(this, OnUpdate);
        }

        private void OnUpdate()
        {
            if (Application.targetFrameRate != _frameRate)
                Application.targetFrameRate = _frameRate;
        }
    }
}