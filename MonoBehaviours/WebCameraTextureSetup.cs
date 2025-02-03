using System;
using UnityEngine;

namespace MobileExerciseAnalyzerView.DebugElements
{
    public class WebCameraTextureSetup : MonoBehaviour
    {
        [SerializeField] private Material _material;
        private WebCamTexture _webCamTexture;

        private void Update()
        {
            // Check if at least one webcam is available
            try
            {
                if (WebCamTexture.devices.Length > 0)
                {
                    _webCamTexture ??= new WebCamTexture();
                    _webCamTexture.Play();
                    if (!_webCamTexture.isPlaying)
                        throw new Exception("Cannot play webcam texture");
                    _material.mainTexture = _webCamTexture;
                }
                else
                {
                    Debug.LogWarning("No webcam detected on this device.");
                    _webCamTexture = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                enabled = false;
            }
        }
    }
}