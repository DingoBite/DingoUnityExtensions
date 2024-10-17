using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RotaryHeart.Lib.AutoComplete
{
    public static class AutoCompleteDropDown
    {
        #if UNITY_EDITOR
        
        /// <summary>
        /// Uses UnityEditor.EditorGUILayout to draw the text field
        /// </summary>
        public static class EditorGUILayout
        {
            #region Polymorphism
            
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                AutoCompleteDropDown("", text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom, allowEmpty, orderFunc, options);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                AutoCompleteDropDown("", text, style, entries, onItemAdded, allowCustom, allowEmpty, orderFunc, options);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(string label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                AutoCompleteDropDown(label, text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom, allowEmpty, orderFunc, options);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(string label, string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                AutoCompleteDropDown(new GUIContent(label), text, style, entries, onItemAdded, allowCustom, allowEmpty, orderFunc, options);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(GUIContent label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                AutoCompleteDropDown(label, text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom, allowEmpty, orderFunc, options);
            }
            
            #endregion Polymorphism

            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="options">
            /// An optional list of layout options that specify extra layouting properties.<para>&#160;</para>
            /// Any values passed in here will override settings defined by the style.<para>&#160;</para>
            /// See Also: GUILayout.Width, GUILayout.Height, GUILayout.MinWidth, GUILayout.MaxWidth, GUILayout.MinHeight, GUILayout.MaxHeight, GUILayout.ExpandWidth, GUILayout.ExpandHeight.
            /// </param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(GUIContent label, string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null, params GUILayoutOption[] options)
            {
                //Get the rect to draw the text field
                Rect lastRect = UnityEditor.EditorGUILayout.GetControlRect(!string.IsNullOrEmpty(label.text), UnityEditor.EditorGUIUtility.singleLineHeight, style, options);

                //Draw it without using layout
                EditorGUI.AutoCompleteDropDown(lastRect, label, text, style, entries, onItemAdded, allowCustom: allowCustom, allowEmpty: allowEmpty, orderFunc);
            }
        }

        /// <summary>
        /// Uses UnityEditor.EditorGUI to draw the text field
        /// </summary>
        public static class EditorGUI
        {
            #region Polymorphism
            
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                AutoCompleteDropDown(position, "", text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom, allowEmpty, orderFunc);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                 AutoCompleteDropDown(position, "", text, style, entries, onItemAdded, allowCustom, allowEmpty, orderFunc);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                AutoCompleteDropDown(position, label, text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom, allowEmpty, orderFunc);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string label, string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                AutoCompleteDropDown(position, new GUIContent(label), text, style, entries, onItemAdded, allowCustom: allowCustom, allowEmpty: allowEmpty, orderFunc);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, GUIContent label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                AutoCompleteDropDown(position, label, text, AutoCompleteBase.M_dropdownStyle, entries, onItemAdded, allowCustom: allowCustom, allowEmpty: allowEmpty, orderFunc);
            }
            #endregion Polymorphism

            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="orderFunc"></param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, GUIContent label, string text, GUIStyle style, string[] entries, Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, Func<string, float> orderFunc = null)
            {
                Rect pos = UnityEditor.EditorGUI.PrefixLabel(position, label);
                
                UnityEngine.GUI.SetNextControlName("CheckFocus");
                
                if (UnityEngine.GUI.Button(pos, new GUIContent(text), style))
                {
                    UnityEngine.GUI.FocusControl("CheckFocus");
                }

                position.x = pos.x;
                AutoCompleteBase._AutoCompleteLogic(position, label, text, entries, allowCustom, allowEmpty, true, null, onItemAdded, orderFunc:orderFunc);
            }
        }

#endif

        public static class GUI
        {
            #region Polymorphism
            
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null)
            {
                AutoCompleteDropDown(position, "", text, UnityEngine.GUI.skin.button, entries, onItemAdded, allowCustom, allowEmpty, windowStyle);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="style">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string text, GUIStyle style, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null)
            {
                AutoCompleteDropDown(position, "", text, style, entries, onItemAdded, allowCustom, allowEmpty, windowStyle);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null)
            {
                AutoCompleteDropDown(position, label, text, UnityEngine.GUI.skin.button, entries, onItemAdded, allowCustom, allowEmpty, windowStyle);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="inputStyle">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, string label, string text, GUIStyle inputStyle, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null)
            {
                AutoCompleteDropDown(position, new GUIContent(label), text, inputStyle, entries, onItemAdded, allowCustom: allowCustom, allowEmpty: allowEmpty, windowStyle: windowStyle);
            }
            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, GUIContent label, string text, string[] entries, System.Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null)
            {
                AutoCompleteDropDown(position, label, text, UnityEngine.GUI.skin.button, entries, onItemAdded, allowCustom: allowCustom, allowEmpty: allowEmpty, windowStyle: windowStyle);
            }
            
            #endregion Polymorphism

            /// <summary>
            /// Make a Dropdown that has an <paramref name="AutoCompleteWindow"/> logic for selecting options.
            /// </summary>
            /// <param name="position">Rectangle on the screen to use for the text field</param>
            /// <param name="label">Optional label to display in front of the text field</param>
            /// <param name="text">The text to edit</param>
            /// <param name="inputStyle">Optional GUIStyle</param>
            /// <param name="entries">Entries to display</param>
            /// <param name="onItemAdded">Action called when an item is clicked</param>
            /// <param name="orderFunc"></param>
            /// <param name="allowCustom">Should the system allow custom entries</param>
            /// <param name="allowEmpty">Should the system add a Nothing element and allow returning an empty string</param>
            /// <param name="windowStyle">Contains the style to use for the window (colors, textures, etc)</param>
            /// <returns>Selected value from autocomplete window</returns>
            public static void AutoCompleteDropDown(Rect position, GUIContent label, string text, GUIStyle inputStyle, string[] entries, Action<string> onItemAdded, bool allowCustom = false, bool allowEmpty = true, IStyle windowStyle = null, Func<string,float> orderFunc = null)
            {
                Rect labelPos = new Rect(position.position, inputStyle.CalcSize(label));
                UnityEngine.GUI.Label(labelPos, label, inputStyle);
                position.x = labelPos.xMax;
                position.width -= labelPos.width;
                
                UnityEngine.GUI.SetNextControlName("CheckFocus");
                
                if (UnityEngine.GUI.Button(position, new GUIContent(text), inputStyle))
                {
                    UnityEngine.GUI.FocusControl("CheckFocus");
                }
                
                position.y = position.yMax;
                AutoCompleteBase._AutoCompleteLogic(position, label, text, entries, allowCustom, allowEmpty, false, null, onItemAdded, orderFunc:orderFunc);
            }
        }
    }
}