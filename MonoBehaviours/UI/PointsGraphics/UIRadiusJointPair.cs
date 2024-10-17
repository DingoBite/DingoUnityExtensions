using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIRadiusJointPair : UIBehaviour, IAddictiveColorComponent, ILineComponent, IRedrawable
    {
        [field: SerializeField] public UIRadiusJoint P1 { get; private set; }
        [field: SerializeField] public UIRadiusJoint P2 { get; private set; }

        [SerializeField] private UIJointConnection _jointConnection;
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
            set => _start = value;
        }

        public Vector2 End
        {
            get => _end;
            set => _end = value;
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

        public void SetPoints(Vector2 start, Vector2 end)
        {
            var rect = RectTransform.rect;
            var w = rect.width;
            var h = rect.height;
            
            P1.RectTransform.anchoredPosition = start;
            P2.RectTransform.anchoredPosition = end;

            var dir = end - start;
            start -= dir.normalized * _p1Shoulder;
            end += dir.normalized * _p2Shoulder;

            var p1Offset = P1.OuterJoint.GetLookClosestPoint(end, start);
            var p2Offset = P2.OuterJoint.GetLookClosestPoint(start, end);
            
            var uvP1 = start + rect.center - p1Offset;
            var uvP2 = end + rect.center - p2Offset;


            uvP1 = new Vector2(uvP1.x / w, uvP1.y / h);
            uvP2 = new Vector2(uvP2.x / w, uvP2.y / h);
            
            _jointConnection.SetPoints(uvP1, uvP2);
        }

        private void OnValidate()
        {
            Redraw();
        }

        public void Redraw()
        {
            Color = _color;
            AddictiveColor = _connectionColor;
            Thickness = _connectionThickness;
            SetPoints(P1.RectTransform.anchoredPosition, P2.RectTransform.anchoredPosition);
        }
    }
}