using UnityEngine;
using uPalette.Runtime.Core.Synchronizer.UnityObject;

namespace DingoUnityExtensions.uPaletteExtensions.UnityObjectSynchronizers
{
    [RequireComponent(typeof(MeshFilter))]
    [UnityObjectSynchronizer(typeof(MeshFilter), "Mesh")]
    public class MeshFilterSynchronizer : UnityObjectSynchronizer<MeshFilter>
    {
        [SerializeField] private bool _sharedMeshInPlayMode;
        
        public override Object GetValue()
        {
            if (Application.isPlaying && !_sharedMeshInPlayMode)
                return Component.mesh;
            else 
                return Component.sharedMesh;
        }

        public override void SetValue(Object value)
        {
            if (Application.isPlaying && !_sharedMeshInPlayMode)
                Component.mesh = value as Mesh;
            else 
                Component.sharedMesh = value as Mesh;
        }
    }
}