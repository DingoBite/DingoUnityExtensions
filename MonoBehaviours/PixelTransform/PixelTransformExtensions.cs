using DingoUnityExtensions.Extensions;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform
{
    public static class PixelTransformExtensions
    {
        public static Vector2 GetSpritePivotPixels(this Sprite sprite, PivotPreset pivotPreset, Vector2 custom)
        {
            var sizePx = sprite.rect.size;
            var w = sizePx.x;
            var h = sizePx.y;

            return pivotPreset switch
            {
                PivotPreset.SpritePivot => sprite.pivot,
                PivotPreset.Center => new Vector2(w * 0.5f, h * 0.5f),

                PivotPreset.BottomLeft => new Vector2(0f, 0f),
                PivotPreset.Bottom => new Vector2(w * 0.5f, 0f),
                PivotPreset.BottomRight => new Vector2(w, 0f),

                PivotPreset.Left => new Vector2(0f, h * 0.5f),
                PivotPreset.Right => new Vector2(w, h * 0.5f),

                PivotPreset.TopLeft => new Vector2(0f, h),
                PivotPreset.Top => new Vector2(w * 0.5f, h),
                PivotPreset.TopRight => new Vector2(w, h),

                PivotPreset.Custom => custom,

                _ => sprite.pivot
            };
        }

        public static Vector2Int GetFramePivotPx(this Vector2Int sizePx, PivotPreset pivotPreset, Vector2Int custom)
        {
            var w = (float)sizePx.x;
            var h = (float)sizePx.y;

            return pivotPreset switch
            {
                PivotPreset.SpritePivot => new Vector2Int(Mathf.RoundToInt(w * 0.5f), Mathf.RoundToInt(h * 0.5f)),
                PivotPreset.Center => new Vector2Int(Mathf.RoundToInt(w * 0.5f), Mathf.RoundToInt(h * 0.5f)),

                PivotPreset.BottomLeft => new Vector2Int(0, 0),
                PivotPreset.Bottom => new Vector2Int(Mathf.RoundToInt(w * 0.5f), 0),
                PivotPreset.BottomRight => new Vector2Int(Mathf.RoundToInt(w), 0),

                PivotPreset.Left => new Vector2Int(0, Mathf.RoundToInt(h * 0.5f)),
                PivotPreset.Right => new Vector2Int(Mathf.RoundToInt(w), Mathf.RoundToInt(h * 0.5f)),

                PivotPreset.TopLeft => new Vector2Int(0, Mathf.RoundToInt(h)),
                PivotPreset.Top => new Vector2Int(Mathf.RoundToInt(w * 0.5f), Mathf.RoundToInt(h)),
                PivotPreset.TopRight => new Vector2Int(Mathf.RoundToInt(w), Mathf.RoundToInt(h)),

                PivotPreset.Custom => custom,
                _ => new Vector2Int(Mathf.RoundToInt(w * 0.5f), Mathf.RoundToInt(h * 0.5f))
            };
        }
        
        public static Vector2 GetAnchorNormalized(this AnchorPreset anchorPreset, Vector2 custom)
        {
            return anchorPreset switch
            {
                AnchorPreset.BottomLeft => new Vector2(0f, 0f),
                AnchorPreset.Bottom => new Vector2(0.5f, 0f),
                AnchorPreset.BottomRight => new Vector2(1f, 0f),
                AnchorPreset.Left => new Vector2(0f, 0.5f),
                AnchorPreset.Center => new Vector2(0.5f, 0.5f),
                AnchorPreset.Right => new Vector2(1f, 0.5f),
                AnchorPreset.TopLeft => new Vector2(0f, 1f),
                AnchorPreset.Top => new Vector2(0.5f, 1f),
                AnchorPreset.TopRight => new Vector2(1f, 1f),
                AnchorPreset.Custom => custom.Clamp01(),
                _ => new Vector2(0.5f, 0.5f)
            };
        }
        
        public static bool TryGetSpriteWorldCorners(this Transform tr, SpriteRenderer sr, out Vector3 bl, out Vector3 br, out Vector3 trc, out Vector3 tl)
        {
            bl = br = trc = tl = default;

            var sprite = sr != null ? sr.sprite : null;
            if (sprite == null)
                return false;

            var rect = sprite.rect;
            var pivotPx = sprite.pivot;
            var ppu = Mathf.Max(0.0001f, sprite.pixelsPerUnit);

            var blLocal = new Vector3((-pivotPx.x) / ppu, (-pivotPx.y) / ppu, 0f);
            var brLocal = new Vector3((rect.width - pivotPx.x) / ppu, (-pivotPx.y) / ppu, 0f);
            var tlLocal = new Vector3((-pivotPx.x) / ppu, (rect.height - pivotPx.y) / ppu, 0f);
            var trLocal = new Vector3((rect.width - pivotPx.x) / ppu, (rect.height - pivotPx.y) / ppu, 0f);

            bl = tr.TransformPoint(blLocal);
            br = tr.TransformPoint(brLocal);
            tl = tr.TransformPoint(tlLocal);
            trc = tr.TransformPoint(trLocal);
            return true;
        }
    }
}