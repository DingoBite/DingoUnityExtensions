using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Editor
{
    [CustomEditor(typeof(PixelSnappedTransform))]
    public class PixelSnappedTransformEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var t = (PixelSnappedTransform)target;

            var ppc = t.GetPixelPerfectCamera();
            if (ppc == null)
                return;

            var cam = ppc.GetComponent<Camera>();
            if (cam == null)
                return;

            float ppu = Mathf.Max(1, ppc.assetsPPU);
            var halfW = (ppc.refResolutionX * 0.5f) / ppu;
            var halfH = (ppc.refResolutionY * 0.5f) / ppu;

            var camPos = cam.transform.position;
            camPos.z = 0f;

            Handles.color = Color.cyan;
            Handles.DrawWireCube(camPos, new Vector3(halfW * 2f, halfH * 2f, 0f));

            var pivotWorld = GetPivotWorldFromComponent(t, ppc, cam);

            Handles.color = Color.yellow;
            EditorGUI.BeginChangeCheck();
            var handleSize = HandleUtility.GetHandleSize(pivotWorld) * 0.1f;
            var offset = new Vector3(handleSize, handleSize, 0);
            var newPivotWorld = Handles.FreeMoveHandle(pivotWorld + offset, handleSize, Vector3.zero, Handles.DotHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Move Pixel Anchored Position");

                var delta = newPivotWorld - pivotWorld;
                var deltaPx = new Vector2Int(Mathf.RoundToInt(delta.x * ppu), Mathf.RoundToInt(delta.y * ppu));

                t.AddAnchoredPixels(deltaPx);

                EditorUtility.SetDirty(t);
            }
        }

        static Vector3 GetPivotWorldFromComponent(PixelSnappedTransform t, PixelPerfectCamera ppc, Camera cam)
        {
            return t.transform.position;
        }
    }
}