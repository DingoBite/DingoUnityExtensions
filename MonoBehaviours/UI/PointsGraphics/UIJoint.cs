using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public abstract class UIJoint : UIBehaviour, IRadiusPointComponent, IColorComponent, IAlphaComponent, IRedrawable
    {
        [SerializeField, Min(0)] private float _radius;
        [SerializeField, Range(0, 1)] private float _alpha;

        public float Radius
        {
            get => _radius;
            set => _radius = Math.Max(value, 0);
        }

        public Vector2 Position => RectTransform.anchoredPosition;
        public abstract Color Color { get; set; }
        
        public virtual float Alpha
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

        public virtual Vector2 GetLookClosestPoint(Vector2 p1, Vector2 p2) => MathUtils.GetLookClosestPoint(Radius, p1, p2);

        public virtual Vector2 GetTouchPoint(Vector2 p1, Vector2 p2) => MathUtils.GetTouchPoint(Radius, p1, p2);
        
        public virtual void Redraw()
        {
            var graphicRectTransform = RectTransform;
            graphicRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Radius * 2);
            graphicRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Radius * 2);
            Alpha = _alpha;
            Radius = _radius;
        }

        protected virtual void OnValidate()
        {
            Redraw();
        }
    }
}