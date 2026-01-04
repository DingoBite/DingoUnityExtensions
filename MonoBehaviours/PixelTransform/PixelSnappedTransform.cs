using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DingoUnityExtensions.MonoBehaviours.PixelTransform
{
    [ExecuteAlways]
    public sealed class PixelSnappedTransform : SubscribableBehaviour, IPixelSnappedTransform
    {
        private enum AnchorPreset
        {
            BottomLeft,
            Bottom,
            BottomRight,
            Left,
            Center,
            Right,
            TopLeft,
            Top,
            TopRight,
            Custom
        }

        private enum PivotPreset
        {
            SpritePivot,
            Center,
            BottomLeft,
            Bottom,
            BottomRight,
            Left,
            Right,
            TopLeft,
            Top,
            TopRight,
            Custom
        }

        [Header("Runtime")] 
        [SerializeField] private bool _snapInEditMode = true;
        [SerializeField] private bool _snapInPlayMode = true;

        [Header("Transform management")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [SerializeField] private Transform _parentOverride;
        [SerializeField] private bool _manageScale = true;
        [SerializeField] private bool _ignoreParentScale = true;
        [SerializeField] private bool _managePosition = true;

        [Header("RectTransform-like positioning (reference pixels)")] 
        [SerializeField] private PixelPerfectCamera _ppc;
        [SerializeField] private Vector2 _anchoredPositionPx;

        [SerializeField] private AnchorPreset _anchor = AnchorPreset.Center;
        [SerializeField] private Vector2 _customAnchorNormalized = new(0.5f, 0.5f);
        [SerializeField] private PivotPreset _pivot = PivotPreset.SpritePivot;
        [SerializeField] private Vector2Int _customPivotPx;

        [Header("Pixel scale")] 
        [SerializeField] private Vector2Int _pixelScale = Vector2Int.one;

        [SerializeField] private bool _finalRoundToPixel = true;

        [Header("Layout frame (when no sprite)")] 
        [SerializeField] private Vector2Int _frameSizePx = new(64, 64);

        [SerializeField] private bool _drawFrameGizmo = true;

        private Camera _camera;

        public void SetupPixelPerfectCamera(Camera c)
        {
            _ppc = c.GetComponent<PixelPerfectCamera>();
            _camera = c;
        }

        public void SetPositionPx(Vector2Int position)
        {
            _anchoredPositionPx = position;
            SnapPosition();
        }
        
        public void SetPosition(Vector2 position)
        {
            _anchoredPositionPx = position;
            SnapPosition();
        }

        public void AddPosition(Vector2 delta)
        {
            _anchoredPositionPx += delta;
            SnapPosition();
        }
        
        public Vector2 GetAnchoredPosition() => _anchoredPositionPx;
        public Vector2Int GetAnchoredPixels() => new(Mathf.RoundToInt(_anchoredPositionPx.x), Mathf.RoundToInt(_anchoredPositionPx.y));
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!Application.isPlaying && _snapInEditMode)
                SnapPosition();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying || !_snapInEditMode)
                return;

            SnapPosition();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawFrameGizmo)
                return;

            if (!TryGetFrameWorldCorners(out var bl, out var br, out var tr, out var tl))
                return;

            Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);

            EnsureCamera();
            var ppu = (_ppc != null) ? Mathf.Max(1, _ppc.assetsPPU) : Mathf.Max(1, Mathf.RoundToInt(_spriteRenderer.sprite.pixelsPerUnit));

            var wWorld = Vector3.Distance(bl, br);
            var hWorld = Vector3.Distance(bl, tl);
            var wPx = Mathf.RoundToInt(wWorld * ppu);
            var hPx = Mathf.RoundToInt(hWorld * ppu);

            var rightDir = (br - bl).normalized;
            var upDir = (tl - bl).normalized;

            var bottomMid = (bl + br) * 0.5f;
            var leftMid = (bl + tl) * 0.5f;

            var labelOffsetWorld = (2f / ppu);

            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            Handles.Label(bottomMid + upDir * labelOffsetWorld, wPx.ToString(), style);
            Handles.Label(leftMid + rightDir * labelOffsetWorld, hPx.ToString(), style);
        }
#endif


        private void EnsureCamera()
        {
            if (_ppc != null && _camera != null)
                return;
            if (_ppc == null)
                return;
            _camera = _ppc.GetComponent<Camera>();
        }

        private void SnapPosition()
        {
            EnsureCamera();
            if (_ppc == null || _camera == null)
                return;

            if (_managePosition)
            {
                transform.position = ComputeWorldFromAnchoredPixels();

                if (_finalRoundToPixel)
                    transform.position = _ppc.RoundToPixel(transform.position);
            }

            if (_manageScale)
            {
                if (_spriteRenderer != null)
                {
                    var sx = _pixelScale.x * _spriteRenderer.sprite.pixelsPerUnit / _ppc.assetsPPU;
                    var sy = _pixelScale.y * _spriteRenderer.sprite.pixelsPerUnit / _ppc.assetsPPU;
                    var newScale = new Vector3(sx, sy, transform.localScale.z);

                    var parent = _parentOverride == null ? transform.parent : _parentOverride;
                    if (_ignoreParentScale && parent != null)
                    {
                        newScale.x /= parent.lossyScale.x;
                        newScale.y /= parent.lossyScale.y;
                        newScale.z /= parent.lossyScale.z;
                    }

                    transform.localScale = newScale;
                }
                else
                {
                    transform.localScale = new Vector3(_pixelScale.x, _pixelScale.y, transform.localScale.z);
                }
            }
        }

        private Vector3 ComputeWorldFromAnchoredPixels()
        {
            var desiredWorldPivotPoint = ComputeDesiredWorldPivotPoint();
            var pivotWorldDelta = GetChosenPivotWorldDeltaFromSpritePivot();
            return desiredWorldPivotPoint - new Vector3(pivotWorldDelta.x, pivotWorldDelta.y, 0f);
        }

        private Vector3 ComputeDesiredWorldPivotPoint()
        {
            var ppu = Mathf.Max(1, _ppc.assetsPPU);

            var a = GetAnchorNormalized();

            if (TryGetParentFrame(out var pBl, out var pRightVec, out var pUpVec, out var pRightDir, out var pUpDir))
            {
                var anchorWorld = pBl + pRightVec * a.x + pUpVec * a.y;
                var offsetWorld = pRightDir * (_anchoredPositionPx.x / ppu) + pUpDir * (_anchoredPositionPx.y / ppu);

                var desired = anchorWorld + offsetWorld;
                desired.z = transform.position.z;
                return desired;
            }

            var refX = _ppc.refResolutionX;
            var refY = _ppc.refResolutionY;

            var camRight = _camera.transform.right;
            var camUp = _camera.transform.up;

            var halfW = (refX * 0.5f) / ppu;
            var halfH = (refY * 0.5f) / ppu;

            var camPos = _camera.transform.position;

            var bottomLeft = camPos - camRight * halfW - camUp * halfH;
            var rightVec = camRight * (refX / (float)ppu);
            var upVec = camUp * (refY / (float)ppu);

            var anchorWorld2 = bottomLeft + rightVec * a.x + upVec * a.y;
            var offsetWorld2 = camRight * (_anchoredPositionPx.x / ppu) + camUp * (_anchoredPositionPx.y / ppu);

            var desired2 = anchorWorld2 + offsetWorld2;
            desired2.z = transform.position.z;
            return desired2;
        }

        public bool TryGetFrameWorldCorners(out Vector3 bl, out Vector3 br, out Vector3 tr, out Vector3 tl)
        {
            bl = br = tr = tl = default;

            if (_spriteRenderer != null && _spriteRenderer.sprite != null)
                return TryGetSpriteWorldCorners(transform, _spriteRenderer, out bl, out br, out tr, out tl);

            EnsureCamera();
            if (_ppc == null)
                return false;

            if (_frameSizePx.x <= 0 || _frameSizePx.y <= 0)
                return false;

            var ppu = Mathf.Max(1, _ppc.assetsPPU);

            var right = transform.right * transform.lossyScale.x;
            var up = transform.up * transform.lossyScale.y;

            var wWorld = _frameSizePx.x / (float)ppu;
            var hWorld = _frameSizePx.y / (float)ppu;

            var pivotPx = GetFramePivotPx(_frameSizePx);
            var pivotWorld = (pivotPx.x / (float)ppu) * right + (pivotPx.y / (float)ppu) * up;

            bl = transform.position - pivotWorld;
            br = bl + right * wWorld;
            tl = bl + up * hWorld;
            tr = br + up * hWorld;
            return true;
        }

        private Vector2Int GetFramePivotPx(Vector2Int sizePx)
        {
            var w = (float)sizePx.x;
            var h = (float)sizePx.y;

            return _pivot switch
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

                PivotPreset.Custom => _customPivotPx,
                _ => new Vector2Int(Mathf.RoundToInt(w * 0.5f), Mathf.RoundToInt(h * 0.5f))
            };
        }

        private bool TryGetParentFrame(out Vector3 bl, out Vector3 rightVec, out Vector3 upVec, out Vector3 rightDir, out Vector3 upDir)
        {
            bl = default;
            rightVec = default;
            upVec = default;
            rightDir = default;
            upDir = default;

            var p = _parentOverride == null ? transform.parent : _parentOverride;
            if (p == null)
                return false;

            var parentPst = p.GetComponent<PixelSnappedTransform>();
            if (parentPst == null)
                return false;

            if (!parentPst.TryGetFrameWorldCorners(out var pbl, out var pbr, out _, out var ptl))
                return false;

            bl = pbl;
            rightVec = pbr - pbl;
            upVec = ptl - pbl;

            rightDir = rightVec.sqrMagnitude > 0.000001f ? rightVec.normalized : Vector3.right;
            upDir = upVec.sqrMagnitude > 0.000001f ? upVec.normalized : Vector3.up;

            return true;
        }

        private static bool TryGetSpriteWorldCorners(Transform tr, SpriteRenderer sr, out Vector3 bl, out Vector3 br, out Vector3 trc, out Vector3 tl)
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

        private Vector2 GetChosenPivotWorldDeltaFromSpritePivot()
        {
            var sprite = _spriteRenderer != null ? _spriteRenderer.sprite : null;
            if (sprite == null)
                return Vector2.zero;

            var chosenPivotPx = GetChosenPivotPixels(sprite);
            var spritePivotPx = sprite.pivot;

            var deltaLocalUnits = (chosenPivotPx - spritePivotPx) / Mathf.Max(0.0001f, sprite.pixelsPerUnit);

            var s = transform.lossyScale;
            return new Vector2(deltaLocalUnits.x * s.x, deltaLocalUnits.y * s.y);
        }

        private Vector2 GetChosenPivotPixels(Sprite sprite)
        {
            var sizePx = sprite.rect.size;
            var w = sizePx.x;
            var h = sizePx.y;

            return _pivot switch
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

                PivotPreset.Custom => _customPivotPx,

                _ => sprite.pivot
            };
        }

        private Vector2 GetAnchorNormalized()
        {
            return _anchor switch
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
                AnchorPreset.Custom => Clamp01(_customAnchorNormalized),
                _ => new Vector2(0.5f, 0.5f)
            };
        }

        private static Vector2 Clamp01(Vector2 v) => new(Mathf.Clamp01(v.x), Mathf.Clamp01(v.y));

        protected override void SubscribeOnly()
        {
            if (_snapInPlayMode)
                CoroutineParent.AddLateUpdater(this, SnapPosition, CoroutineOrderLayers.MIN_PRIORITY_SPECIAL);
        }

        protected override void UnsubscribeOnly()
        {
            CoroutineParent.RemoveLateUpdater(this);
        }

#if UNITY_EDITOR
        public void Editor_AddAnchoredPixels(Vector2Int deltaPx)
        {
            _anchoredPositionPx += deltaPx;
            SnapPosition();
        }

        public Vector2Int Editor_GetPixelScale() => _pixelScale;

        public void Editor_SetPixelScale(Vector2Int value)
        {
            _pixelScale = new Vector2Int(value.x, value.y);
            SnapPosition();
        }

        public PixelPerfectCamera Editor_GetPixelPerfectCamera()
        {
            EnsureCamera();
            return _ppc;
        }

        public int Editor_GetAssetsPPU()
        {
            EnsureCamera();
            return _ppc != null ? Mathf.Max(1, _ppc.assetsPPU) : 1;
        }

        public Vector3 Editor_GetDesiredWorldPivotPoint()
        {
            EnsureCamera();
            if (_ppc == null || _camera == null)
                return transform.position;
            return ComputeDesiredWorldPivotPoint();
        }
#endif
    }
}