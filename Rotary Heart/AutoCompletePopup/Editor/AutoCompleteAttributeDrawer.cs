using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.AutoComplete
{
    [CustomPropertyDrawer(typeof(AutoCompleteAttribute))]
    [CustomPropertyDrawer(typeof(AutoCompleteTextFieldAttribute))]
    [CustomPropertyDrawer(typeof(AutoCompleteDropDownAttribute))]
    public class AutoCompleteAttributeDrawer : PropertyDrawer
    {
        enum AttributeType
        {
            TextField,
            Dropdown
        }
        
        string[] m_entries;
        AttributeType m_attributeType;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (m_entries == null)
            {
                AutoCompleteAttribute attribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(AutoCompleteAttribute)) as AutoCompleteAttribute;
                if (attribute.MemberName != null)
                    m_entries = (GetValues(property, attribute.MemberName) as IEnumerable<string>)?.ToArray();
                else 
                    m_entries = attribute.Entries;
                
                if (System.Attribute.GetCustomAttribute(fieldInfo, typeof(AutoCompleteTextFieldAttribute)) != null)
                {
                    m_attributeType = AttributeType.TextField;
                }
                else if (System.Attribute.GetCustomAttribute(fieldInfo, typeof(AutoCompleteDropDownAttribute)) != null)
                {
                    m_attributeType = AttributeType.Dropdown;
                }
            }

            Func<string, float> orderFunc;
            switch (m_attributeType)
            {
                case AttributeType.TextField:
                    AutoCompleteTextFieldAttribute textFieldAttribute = attribute as AutoCompleteTextFieldAttribute;
                    orderFunc = GetValues(property, textFieldAttribute.OrderFuncMember) as Func<string, float>;
                    AutoCompleteDropDown.EditorGUI.AutoCompleteDropDown(position, label, property.stringValue, m_entries, s =>
                    {
                        property.stringValue = s;
                        property.serializedObject.ApplyModifiedProperties();
                    }, textFieldAttribute.AllowCustom, textFieldAttribute.AllowEmpty, textFieldAttribute.ReturnFullPath, textFieldAttribute.Separator, orderFunc);
                    property.stringValue = AutoCompleteTextField.EditorGUI.AutoCompleteTextField(position, label, property.stringValue, GUI.skin.textField, m_entries, "Type something here");
                    break;
                case AttributeType.Dropdown:
                    AutoCompleteDropDownAttribute dropDownAttribute = attribute as AutoCompleteDropDownAttribute;
                    orderFunc = GetValues(property, dropDownAttribute.OrderFuncMember) as Func<string, float>;
                    AutoCompleteDropDown.EditorGUI.AutoCompleteDropDown(position, label, property.stringValue, m_entries, s =>
                    {
                        property.stringValue = s;
                        property.serializedObject.ApplyModifiedProperties();
                    }, dropDownAttribute.AllowCustom, dropDownAttribute.AllowEmpty, dropDownAttribute.ReturnFullPath, dropDownAttribute.Separator, orderFunc);
                    break;
            }
        }
        
        private object GetValues(SerializedProperty property, string valuesName)
        {
            object target = PropertyUtility.GetTargetObjectWithProperty(property);

            FieldInfo valuesFieldInfo = ReflectionUtility.GetField(target, valuesName);
            if (valuesFieldInfo != null)
            {
                return valuesFieldInfo.GetValue(target);
            }

            PropertyInfo valuesPropertyInfo = ReflectionUtility.GetProperty(target, valuesName);
            if (valuesPropertyInfo != null)
            {
                return valuesPropertyInfo.GetValue(target);
            }

            MethodInfo methodValuesInfo = ReflectionUtility.GetMethod(target, valuesName);
            if (methodValuesInfo != null &&
                methodValuesInfo.ReturnType != typeof(void) &&
                methodValuesInfo.GetParameters().Length == 0)
            {
                return methodValuesInfo.Invoke(target, null);
            }

            return null;
        }
    }
}