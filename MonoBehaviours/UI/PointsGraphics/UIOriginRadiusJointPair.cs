using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIOriginRadiusJointPair : UIBehaviour, IAddictiveColorComponent, ILineComponent, IRedrawable, IAlphaComponent
    {
        [field: SerializeField] public UISolidJoint P1 { get; private set; }
        [field: SerializeField] public UIRadiusThicknessJoint P2 { get; private set; }

        [SerializeField] private UIJointConnection _jointConnection;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Color _connectionColor = Color.white;
        [SerializeField] private float _connectionThickness;
        [SerializeField] private float _p1Shoulder;
        [SerializeField] private float _p2Shoulder;
        [SerializeField] private Vector2 _start;
        [SerializeField] private Vector2 _end;
        [SerializeField, Range(0, 1)] private float _alpha;

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
            get => _jointConnection.Thickness;
            set => _jointConnection.Thickness = value;
        }

        public Color AddictiveColor
        {
            get => _jointConnection.Color;
            set => _jointConnection.Color = value;
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
        
        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                P1.Alpha = value;
                P2.Alpha = value;
                _jointConnection.Alpha = value;
            }
        }

        public void SetPoints(Vector2 start, Vector2 end)
        {
            var rect = RectTransform.rect;
            var w = rect.width;
            var h = rect.height;
            
            P1.RectTransform.anchoredPosition = start;
            P2.RectTransform.anchoredPosition = end;

            var (p1Offset, p2Offset) = Utils.GetRadiusOffset(P1, P2.OuterJoint, ref start, ref end, _p1Shoulder, _p2Shoulder);

            var pos = Vector2.zero;
            // var pos = rect.position;
            var uvP1 = start - pos + p1Offset;
            var uvP2 = end - pos + p2Offset;


            uvP1 = new Vector2(uvP1.x / w, uvP1.y / h);
            uvP2 = new Vector2(uvP2.x / w, uvP2.y / h);
            
            _jointConnection.SetPoints(uvP1, uvP2);
        }

        private void OnValidate()
        {
            AddictiveColor = _connectionColor;
            Color = _color;
            Thickness = _connectionThickness;
            Alpha = _alpha;
            Redraw();
        }

        public void Redraw()
        {
            SetPoints(_start, _end);
        }
    }
}