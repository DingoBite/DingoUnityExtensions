using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIRadiusThicknessJoint : UIJoint, IOuterJointComponent, IInnerJointComponent, IThicknessComponent, IProgressComponent, IMinRadiusComponent, IRedrawable, IAlphaComponent
    {
        [field: SerializeField] public UIJoint InnerJoint { get; private set; }
        [field: SerializeField] public UIJoint OuterJoint { get; private set; }
        
        [SerializeField, Min(0)] private float _minRadius;
        [SerializeField, Range(0, 1)] private float _progress;
        [SerializeField, Min(0)] private float _outerThickness;
        [SerializeField] private Color _innerColor = Color.white;
        [SerializeField] private Color _outerColor = Color.white;
        [SerializeField] private Color _color = Color.white;
        
        public UIJoint Outer => OuterJoint;
        public UIJoint Inner => InnerJoint;
        
        public float Thickness
        {
            get => _outerThickness;
            set
            {
                _outerThickness = value;
                if (OuterJoint is IThicknessComponent thicknessComponent)
                    thicknessComponent.Thickness = value;
            }
        }
        
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
                value.a = Alpha;
                _color = value;
                OuterJoint.Color = _color * _outerColor;
                InnerJoint.Color = _color * _innerColor; 
            }
        }

        public override float Alpha
        {
            get => base.Alpha;
            set
            {
                base.Alpha = value;
                OuterJoint.Alpha = value;
                InnerJoint.Alpha = value;
            }
        }

        public override void Redraw()
        {
            base.Redraw();
            Thickness = _outerThickness;
            OuterJoint.Radius = Radius;
            MinRadius = _minRadius;
            Progress = _progress;
            if (OuterJoint is IRadiusWithoutThicknessComponent radiusWithoutThicknessComponent)
                InnerJoint.Radius = _minRadius + (radiusWithoutThicknessComponent.RadiusWithoutThickness - _minRadius) * _progress;
            Color = _color;
            InnerJoint.Redraw();
            OuterJoint.Redraw();
        }
    }
}