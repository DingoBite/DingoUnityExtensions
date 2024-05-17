using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using AYellowpaper.SerializedCollections.Editor;
using UnityEngine;
using UnityEditor;

namespace NaughtyAttributes.Editor
{
    [CustomPropertyDrawer(typeof(AllowNestingAttribute))]
    public class AllowNestingPropertyDrawer : PropertyDrawerBase
    {
        private Dictionary<string, SerializedDictionaryInstanceDrawer> _arrayData = new();
        
        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            var targetObjectOfProperty = PropertyUtility.GetTargetObjectOfProperty(property);
            if (targetObjectOfProperty != null && targetObjectOfProperty.GetType().Namespace == typeof(SerializedDictionary<,>).Namespace)
            {
                if (!_arrayData.ContainsKey(property.propertyPath))
                    _arrayData.Add(property.propertyPath, new SerializedDictionaryInstanceDrawer(property, fieldInfo));
                _arrayData[property.propertyPath].OnGUI(rect, label);
            }
            else
            {
                EditorGUI.BeginProperty(rect, label, property);
                EditorGUI.PropertyField(rect, property, label, true);
                EditorGUI.EndProperty();
            }
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            var targetObjectOfProperty = PropertyUtility.GetTargetObjectOfProperty(property);
            if (targetObjectOfProperty != null && targetObjectOfProperty.GetType().Namespace == typeof(SerializedDictionary<,>).Namespace)
            {
                if (!_arrayData.ContainsKey(property.propertyPath))
                    _arrayData.Add(property.propertyPath, new SerializedDictionaryInstanceDrawer(property, fieldInfo));
                return _arrayData[property.propertyPath].GetPropertyHeight(label);
            }

            return base.GetPropertyHeight_Internal(property, label);
        }
    }
}
