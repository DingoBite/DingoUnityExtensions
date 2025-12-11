using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIJointConnection : UIBehaviour, IThicknessComponent, ILineComponent, IColorComponent, IClearable, IAlphaComponent
    {
        [SerializeField] private UIGraph.UIGraph _graph;
        [SerializeField] private Color _color;
        [SerializeField] private Vector2 _start;
        [SerializeField] private Vector2 _end;
        [SerializeField, Range(0, 1)] private float _alpha;
        
        public Gradient LineGradient => _graph.Gradient;

        public Vector2 Start
        {
            get => _start;
            set
            {
                _start = value;
                SetPoints(_start, _end);
            }
        }

        public Vector2 End
        {
            get => _end;
            set
            {
                _end = value;
                SetPoints(_start, _end);
            }
        }

        public float Thickness
        {
            get => _graph.Thickness;
            set => _graph.Thickness = value;
        }

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                _graph.color = value;
            }
        }

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                var c = Color;
                c.a = value;
                Color = c;
            }
        }
        
        public void Clear()
        {
            _graph.Clear();
        }
        
        public void SetPoints(Vector2 p1, Vector2 p2)
        {
            _start = p1;
            _end = p2;
            _graph.Clear();
            _graph.AddPoint(p1);
            _graph.AddPoint(p2);
            _graph.SetDirty();
        }

        private void OnValidate()
        {
            Color = _color;
            Alpha = _alpha;
            SetPoints(_start, _end);
        }
    }
}