using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform.Editor
{
    [EditorTool("Pixel Snap Transform Tool", typeof(PixelSnappedTransform))]
    public class PixelSnappedTransformTool : EditorTool
    {
        private GUIContent _icon;

        private void OnEnable()
        {
            _icon = EditorGUIUtility.IconContent("ScaleTool");
            _icon.text = "PixelSnap";
            _icon.tooltip = "Move/Scale in pixel grid (updates anchoredPositionPx and pixelScale)";
        }

        public override GUIContent toolbarIcon => _icon;

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var o in targets)
            {
                var t = o as PixelSnappedTransform;
                if (t == null)
                    continue;

                var ppu = t.Editor_GetAssetsPPU();
                var snapStepWorld = 1f / Mathf.Max(1, ppu);

                var pivotWorld = t.Editor_GetDesiredWorldPivotPoint();

                Handles.color = Color.yellow;
                EditorGUI.BeginChangeCheck();

                var newPivotWorld = Handles.FreeMoveHandle(pivotWorld, HandleUtility.GetHandleSize(pivotWorld) * 0.08f, new Vector3(snapStepWorld, snapStepWorld, snapStepWorld), Handles.DotHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Move Pixel Anchored Position");

                    var d = newPivotWorld - pivotWorld;
                    var deltaPx = new Vector2Int(Mathf.RoundToInt(d.x * ppu), Mathf.RoundToInt(d.y * ppu));

                    t.Editor_AddAnchoredPixels(deltaPx);
                    EditorUtility.SetDirty(t);
                }

                var cur = t.Editor_GetPixelScale();
                var curScale = new Vector3(cur.x, cur.y, 1f);

                Handles.color = Color.cyan;
                EditorGUI.BeginChangeCheck();

                var newScale = Handles.ScaleHandle(curScale, t.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(t.transform.position));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Scale Pixel Sprite");

                    var sx = Mathf.RoundToInt(newScale.x);
                    var sy = Mathf.RoundToInt(newScale.y);

                    t.Editor_SetPixelScale(new Vector2Int(sx, sy));
                    EditorUtility.SetDirty(t);
                }
            }
        }
    }
}