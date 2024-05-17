using System;
using NaughtyAttributes.ColorKeyProperties;
using Unity.Attributes.NaughtyAttributes.Scripts.Core;
using UnityEngine;

namespace NaughtyAttributes
{
    public enum EButtonEnableMode
    {
        /// <summary>
        /// Button should be active always
        /// </summary>
        Always,
        /// <summary>
        /// Button should be active only in editor
        /// </summary>
        Editor,
        /// <summary>
        /// Button should be active only in playmode
        /// </summary>
        Playmode
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
    public class ColorKeyAttribute : Attribute, INaughtyAttribute
    {
        public ColorPart Part { get; private set; }
        public Color Color { get; private set; }

        public ColorKeyAttribute(float r, float g, float b, ColorPart part)
        {
            Color = new Color(r, g, b);
            Part = part;
        }
        
        public ColorKeyAttribute(byte r, byte g, byte b, ColorPart part)
        {
            Color = new Color32(r, g, b, 255);
            Part = part;
        }
        
        public ColorKeyAttribute(ColorEnum colorEnum, ColorPart part)
        {
            Color = Colors.ColorDict[colorEnum];
            Part = part;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : SpecialCaseDrawerAttribute
    {
        public string Text { get; private set; }
        public EButtonEnableMode SelectedEnableMode { get; private set; }
        public FontStyle FontStyle { get; private set; }

        public ButtonAttribute(string text = null, EButtonEnableMode enabledMode = EButtonEnableMode.Always, FontStyle fontStyle = FontStyle.Normal)
        {
            this.Text = text;
            this.SelectedEnableMode = enabledMode;
            this.FontStyle = fontStyle;
        }
    }
}
