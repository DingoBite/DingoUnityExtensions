using UnityEngine;
using UnityEngine.Rendering;

namespace DingoUnityExtensions.LightUtils
{
    public static class LightingApplier
    {
        public static void Apply(this GlobalLightingPreset p, Volume globalVolume = null)
        {
            if (p.Skybox)
                RenderSettings.skybox = p.Skybox;
            RenderSettings.ambientMode = p.AmbientMode;
            RenderSettings.ambientLight = p.AmbientColor;
            RenderSettings.ambientIntensity = p.AmbientIntensity;
            RenderSettings.subtractiveShadowColor = p.RealtimeShadowColor;
            
            RenderSettings.fog = p.Fog;
            RenderSettings.fogColor = p.FogColor;
            RenderSettings.fogDensity = p.FogDensity;

            DynamicGI.UpdateEnvironment();

            if (globalVolume && p.GlobalVolumeProfile)
                globalVolume.sharedProfile = p.GlobalVolumeProfile;
        }
    }
}