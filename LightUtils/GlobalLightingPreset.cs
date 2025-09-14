using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace DingoUnityExtensions.LightUtils
{
    [CreateAssetMenu(menuName = "Lighting/Level Lighting Preset")]
    public class GlobalLightingPreset : ScriptableObject
    {
        public Material Skybox;
        public AmbientMode AmbientMode = AmbientMode.Skybox;
        public Color AmbientColor = Color.gray;
        public float AmbientIntensity = 1f;
        public Color RealtimeShadowColor = Color.gray;

        public Color SkyColor = Color.gray;
        public Color EquatorColor = Color.gray;
        public Color GroundColor = Color.gray;
        
        public bool Fog;
        public Color FogColor = Color.gray;
        public float FogDensity = 0.01f;

        public VolumeProfile GlobalVolumeProfile;

        [Button]
        private void CollectFromCurrentSettings()
        {
            Skybox = RenderSettings.skybox;
            AmbientMode = RenderSettings.ambientMode;
            AmbientColor = RenderSettings.ambientLight;
            AmbientIntensity = RenderSettings.ambientIntensity;
            RealtimeShadowColor = RenderSettings.subtractiveShadowColor;

            SkyColor = RenderSettings.ambientSkyColor;
            EquatorColor = RenderSettings.ambientEquatorColor;
            GroundColor = RenderSettings.ambientGroundColor;

            Fog = RenderSettings.fog;
            FogColor = RenderSettings.fogColor;
            FogDensity = RenderSettings.fogDensity;
        }

        [Button]
        private void Apply() => LightingApplier.Apply(this);
    }
}