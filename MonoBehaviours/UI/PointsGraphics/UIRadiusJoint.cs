using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIRadiusJoint : UIJoint, IInnerJointComponent, IOuterJointComponent, IMinRadiusComponent, IProgressComponent
    {
        [field: SerializeField] public UISolidJoint InnerJoint { get; private set; }
        [field: SerializeField] public UISolidJoint OuterJoint { get; private set; }
        
        [SerializeField, Min(0)] private float _minRadius;
        [SerializeField, Range(0, 1)] private float _progress;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private bool _closestJointAtInner;
        
        public UIJoint Outer => OuterJoint;
        public UIJoint Inner => InnerJoint;
        
        public float Progress
        {
            get => _progress;
            set => _progress = Math.Clamp(value, 0, 1);
        }

        public float MinRadius
        {
            get => _minRadius;
            set => _minRadius = Math.Max(value, 0);
        }

        public override Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OuterJoint.Color = _color;
            }
        }

        public override Vector2 GetTouchPoint(Vector2 p1, Vector2 p2)
        {
            if (!_closestJointAtInner)
                return base.GetTouchPoint(p1, p2);

            return MathUtils.GetTouchPoint(MinRadius, p1, p2);
        }

        public override Vector2 GetLookClosestPoint(Vector2 p1, Vector2 p2)
        {
            if (!_closestJointAtInner)
                return base.GetLookClosestPoint(p1, p2);

            return MathUtils.GetLookClosestPoint(MinRadius, p1, p2);
        }

        public override void Redraw()
        {
            base.Redraw();
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Radius * 2);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Radius * 2);
            OuterJoint.Radius = Radius;
            InnerJoint.Radius = _minRadius + (OuterJoint.Radius - _minRadius) * _progress;
            Progress = _progress;
            Color = _color;
            
            InnerJoint.Redraw();
            OuterJoint.Redraw();
        }
    }
}