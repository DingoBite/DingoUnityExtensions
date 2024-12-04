#if UNITY_2019_3_OR_NEWER
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using NaughtyAttributes.Editor;
using Unity.Attributes.NaughtyAttributes.Scripts.Core;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MackySoft.SerializeReferenceExtensions.Editor
{
	public static class TypeExtensions
	{
		public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
		{
			if (!genericType.IsGenericType)
				return givenType.IsAssignableFrom(givenType);
			
			var interfaceTypes = givenType.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
					return true;
			}

			if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
				return true;

			Type baseType = givenType.BaseType;
			if (baseType == null) return false;

			return IsAssignableToGenericType(baseType, genericType);
		}
	}
	
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer
	{
		public static event Action<object, string, Type> PreTypeChangedAtPropertyPath; 
		public static event Action<object, string, Type> AfterTypeChangedAtPropertyPath; 
		
		struct TypePopupCache {
			public AdvancedTypePopup TypePopup { get; }
			public AdvancedDropdownState State { get; }
			public TypePopupCache (AdvancedTypePopup typePopup,AdvancedDropdownState state) {
				TypePopup = typePopup;
				State = state;
			}
		}

		const int k_MaxTypePopupLineCount = 13;
		static readonly Type k_UnityObjectType = typeof(UnityEngine.Object);
		static readonly GUIContent k_NullDisplayName = new GUIContent(TypeMenuUtility.k_NullDisplayName);
		static readonly GUIContent k_IsNotManagedReferenceLabel = new GUIContent("The property type is not manage reference.");

		readonly Dictionary<string,TypePopupCache> m_TypePopups = new Dictionary<string,TypePopupCache>();
		readonly Dictionary<string,GUIContent> m_TypeNameCaches = new Dictionary<string,GUIContent>();

		SerializedProperty m_TargetProperty;

		public override void OnGUI (Rect position,SerializedProperty property,GUIContent label)
		{
			GUI.contentColor = Colors.White;
			label.text = label.text.Split(' ').Last();
			EditorGUI.BeginProperty(position,label,property);
			
			if (property.propertyType == SerializedPropertyType.ManagedReference) {
				// Draw the subclass selector popup.
				var popupPosition = new Rect(position);
				popupPosition.width -= EditorGUIUtility.labelWidth;
				popupPosition.x += EditorGUIUtility.labelWidth;
				popupPosition.height = EditorGUIUtility.singleLineHeight;

				if (EditorGUI.DropdownButton(popupPosition,GetTypeName(property),FocusType.Keyboard))
				{
					var attribute = PropertyUtility.GetAttribute<SubclassSelectorAttribute>(property);
					var popup = GetTypePopup(property, attribute);
					m_TargetProperty = property;
					popup.TypePopup.Show(popupPosition);
				}

				NaughtyEditorGUI.PropertyField(position, property, true);
			} else {
                EditorGUI.LabelField(position,label,k_IsNotManagedReferenceLabel);
			}

			EditorGUI.EndProperty();
		}
        
		PropertyDrawer GetCustomPropertyDrawer (SerializedProperty property)
		{
			Type propertyType = ManagedReferenceUtility.GetType(property.managedReferenceFullTypename);
			if (propertyType != null && PropertyDrawerCache.TryGetPropertyDrawer(propertyType, out PropertyDrawer drawer))
			{
				return drawer;
			}
			return null;
		}
		
		TypePopupCache GetTypePopup(SerializedProperty property, SubclassSelectorAttribute attribute = null) {
			// Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
			string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

			if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename,out TypePopupCache result)) {
				var state = new AdvancedDropdownState();
				
				Type baseType = ManagedReferenceUtility.GetType(managedReferenceFieldTypename);
				var popup = new AdvancedTypePopup(
					TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
						(p.IsPublic || p.IsNestedPublic) &&
						!p.IsAbstract &&
						!p.IsGenericType &&
						!k_UnityObjectType.IsAssignableFrom(p) &&
						(attribute?.TypeAssignableRestriction == null || p.IsAssignableToGenericType(attribute.TypeAssignableRestriction)) && 
						Attribute.IsDefined(p,typeof(SerializableAttribute))
					),
					k_MaxTypePopupLineCount,
					state
				);
				popup.OnItemSelected += item => {
					Type type = item.Type;

					// Apply changes to individual serialized objects.
					foreach (var targetObject in m_TargetProperty.serializedObject.targetObjects) {
						SerializedObject individualObject = new SerializedObject(targetObject);
						SerializedProperty individualProperty = individualObject.FindProperty(m_TargetProperty.propertyPath);

						object obj = individualProperty.SetManagedReference(type);
						individualProperty.isExpanded = (obj != null);

						PreTypeChangedAtPropertyPath?.Invoke(property.serializedObject.targetObject, property.propertyPath, type);
						individualObject.ApplyModifiedProperties();
						individualObject.Update();
						AfterTypeChangedAtPropertyPath?.Invoke(property.serializedObject.targetObject, property.propertyPath, type);
					}
				};

				result = new TypePopupCache(popup, state);
				m_TypePopups.Add(managedReferenceFieldTypename, result);
			}
			return result;
		}

		GUIContent GetTypeName (SerializedProperty property) {
			// Cache this string.
			string managedReferenceFullTypename = property.managedReferenceFullTypename;

			if (string.IsNullOrEmpty(managedReferenceFullTypename)) {
				return k_NullDisplayName;
			}
			if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename,out GUIContent cachedTypeName)) {
				return cachedTypeName;
			}

			Type type = ManagedReferenceUtility.GetType(managedReferenceFullTypename);
			
			string typeName = null;

			AddTypeMenuAttribute typeMenu = TypeMenuUtility.GetAttribute(type);
			if (typeMenu != null) {
				typeName = typeMenu.GetTypeNameWithoutPath();
				if (!string.IsNullOrWhiteSpace(typeName)) {
					typeName = ObjectNames.NicifyVariableName(typeName);
				}
			}

			if (string.IsNullOrWhiteSpace(typeName)) {
				typeName = ObjectNames.NicifyVariableName(type.Name);
			}

			GUIContent result = new GUIContent(typeName);
			m_TypeNameCaches.Add(managedReferenceFullTypename,result);
			return result;
		}

		public override float GetPropertyHeight (SerializedProperty property,GUIContent label) {
			PropertyDrawer customDrawer = GetCustomPropertyDrawer(property);
			if (customDrawer != null)
			{
				return property.isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +  customDrawer.GetPropertyHeight(property,label):EditorGUIUtility.singleLineHeight;
			}
			else
			{
				return property.isExpanded ? EditorGUI.GetPropertyHeight(property,true) : EditorGUIUtility.singleLineHeight;
			}
		}

	}
}
#endif