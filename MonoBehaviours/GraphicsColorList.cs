using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours
{
    public enum ColorProcess
    {
        Set,
        Add,
        Sub,
        Multiply,
        Divide,
        Invert,
    }
    
    [Serializable]
    public struct GraphicColorProcessor
    {
        [SerializeField] private List<ColorProcess> _processes;
        
        public Color Process(in Color baseColor, in Color color)
        {
            var res = baseColor;
            foreach (var colorProcess in _processes)
            {
                try
                {
                    Apply(colorProcess, ref res, baseColor, color);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
            return res;
        }

        private void Apply(ColorProcess colorProcess, ref Color res, in Color baseColor, in Color color)
        {
            switch (colorProcess)
            {
                case ColorProcess.Set:
                    res = color;
                    break;
                case ColorProcess.Add:
                    res = baseColor + color;
                    break;
                case ColorProcess.Sub:
                    res = baseColor - color;
                    break;
                case ColorProcess.Multiply:
                    res = baseColor * color;
                    break;
                case ColorProcess.Divide:
                    for (var i = 0; i < 4; i++)
                    {
                        var v = color[i];
                        if (Math.Abs(v) < Vector2.kEpsilon)
                            res[i] = 0;
                        else
                            res[i] = baseColor[i] / v;
                    }
                    break;
                case ColorProcess.Invert:
                    res = Color.white - color;
                    break;
                default:
                    return;
            }
        }
    }
    
    public class GraphicsColorList : Graphic
    {
        [SerializeField] private SerializedDictionary<Graphic, GraphicColorProcessor> _graphics = new();

        [SerializeField] private bool _debug;
        [SerializeField] private Color _debugColor = Color.white;
        
        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            foreach (var (graphic, properties) in _graphics)
            {
                var processedColor = properties.Process(graphic.color, new Color(0, 0, 0, alpha * color.a));
                graphic.CrossFadeAlpha(processedColor.a, duration, ignoreTimeScale);
            }
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            foreach (var (graphic, properties) in _graphics)
            {
                var processedColor = properties.Process(graphic.color, targetColor * color);
                graphic.CrossFadeColor(processedColor, duration, ignoreTimeScale, useAlpha, useRGB);
            }
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            foreach (var (graphic, properties) in _graphics)
            {
                var processedColor = properties.Process(graphic.color, targetColor * color);
                graphic.CrossFadeColor(processedColor, duration, ignoreTimeScale, useAlpha);
            }
        }

        public override void SetAllDirty()
        {
            if (_debug)
                CrossFadeColor(_debugColor, 0, true, true, true);
            raycastTarget = false;
            base.SetAllDirty();
        }
    }
}