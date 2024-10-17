using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIOriginRadiusBeltJointPair : UIBehaviour, IAddictiveColorComponent, ILineComponent, IRedrawable
    {
        [field: SerializeField] public UISolidJoint P1 { get; private set; }
        [field: SerializeField] public UIRadiusJoint P2 { get; private set; }

        [SerializeField] private UIJointConnection _topBelt;
        [SerializeField] private UIJointConnection _bottomBelt;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Color _connectionColor = Color.white;
        [SerializeField] private float _connectionThickness;
        [SerializeField] private float _p1Shoulder;
        [SerializeField] private float _p2Shoulder;
        [SerializeField] private Vector2 _start;
        [SerializeField] private Vector2 _end;

        public Vector2 Start
        {
            get => _start;
            set
            {
                _start = value;
                Redraw();
            }
        }

        public Vector2 End
        {
            get => _end;
            set
            {
                _end = value;
                Redraw();
            }
        }

        public (float p1, float p2) Shoulders
        {
            get => (_p1Shoulder, _p2Shoulder);
            set => (_p1Shoulder, _p2Shoulder) = value;
        }
        
        public float Thickness
        {
            get => _connectionThickness;
            set
            {
                _topBelt.Thickness = value;
                _bottomBelt.Thickness = value;
            }
        }

        public Color AddictiveColor
        {
            get => _connectionColor;
            set
            {
                _connectionColor = value;
                _topBelt.Color = value;
                _bottomBelt.Color = value;
            }
        }

        public Color Color
        {
            get => _color;
            private set
            {
                P1.Color = value;
                P2.Color = value;
                _color = value;
            }
        }

        public void SetPoints(Vector2 start, Vector2 end)
        {
            var rect = RectTransform.rect;
            var w = rect.width;
            var h = rect.height;
            
            P1.RectTransform.anchoredPosition = start;
            P2.RectTransform.anchoredPosition = end;

            var (startOffset, endOffset) = Utils.GetRadiusTouchOffset(P1, P2.OuterJoint, ref start, ref end, _p1Shoulder, _p2Shoulder);
            SetPoints(_topBelt, start, end, startOffset.top, endOffset.bottom, w, h);
            SetPoints(_bottomBelt, start, end, startOffset.bottom, endOffset.top, w, h);
        }

        private void SetPoints(ILineComponent lineComponent, Vector2 start, Vector2 end, Vector2 top, Vector2 bottom, float w, float h)
        {
            var uvP1 = start + top;
            var uvP2 = end + bottom;

            uvP1 = new Vector2(uvP1.x / w, uvP1.y / h);
            uvP2 = new Vector2(uvP2.x / w, uvP2.y / h);
            
            lineComponent.SetPoints(uvP1, uvP2);
        }
        
        private void OnValidate()
        {
            AddictiveColor = _connectionColor;
            Color = _color;
            Thickness = _connectionThickness;
            Redraw();
        }

        public void Redraw()
        {
            SetPoints(_start, _end);
        }
    }
}