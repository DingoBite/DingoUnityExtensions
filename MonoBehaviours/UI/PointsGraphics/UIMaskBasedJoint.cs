using System;
using DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics.Core;
using NaughtyAttributes;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UIMaskBasedJoint : UISolidJoint, IThicknessComponent, IRadiusWithoutThicknessComponent
    {
        [SerializeField] private RectTransform _jointMask;
        
        [SerializeField, Min(0)] private float _thickness;

        public float RadiusWithoutThickness => Math.Max(Radius - _thickness, 0);

        public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = Math.Clamp(value, 0.001f, Radius);
                Redraw();
            }
        }

        [Button]
        public override void Redraw()
        {
            var parent = RectTransform;
            parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Radius * 2);
            parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Radius * 2);
            
            var graphicRectTransform = Graphic.rectTransform;
            graphicRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Radius * 2);
            graphicRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Radius * 2);

            var maskRect = _jointMask;
            maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RadiusWithoutThickness * 2);
            maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RadiusWithoutThickness * 2);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Thickness = _thickness;
        }
    }
}