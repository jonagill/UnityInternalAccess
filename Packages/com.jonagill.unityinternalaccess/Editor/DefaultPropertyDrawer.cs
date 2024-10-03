using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityInternalAccess.Editor
{
    /// <summary>
    /// Helper class that can be used to invoke SerializedProperty's default type drawer
    /// from within a custom attribute's PropertyDrawer
    /// </summary>
    public static class DefaultPropertyDrawer
    {
        private static readonly Dictionary<SerializedProperty, PropertyDrawer> DrawerDictionary = new Dictionary<SerializedProperty, PropertyDrawer>();
        private static readonly FieldInfo PropertyDrawerFieldInfo;

        static DefaultPropertyDrawer()
        {
            Type propertyDrawer = typeof(PropertyDrawer);
            PropertyDrawerFieldInfo = propertyDrawer.GetField(
                "m_FieldInfo",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void PropertyField(Rect position, SerializedProperty property)
        {
            PropertyField(position, property, GUIContent.none, false);
        }

        public static void PropertyField(Rect position, SerializedProperty property, bool includeChildren)
        {
            PropertyField(position, property, GUIContent.none, includeChildren);
        }

        public static void PropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyField(position, property, label, false);
        }

        public static void PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            var drawer = GetDefaultDrawer(property);
            if (drawer != null)
            {
                drawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, includeChildren);
            }
        }

        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var drawer = GetDefaultDrawer(property);
            if (drawer != null)
            {
                return drawer.GetPropertyHeight(property, label);
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
        }

        // Heavily based on Unity's internal PropertyHandler.HandleDrawnType()
        // https://github.com/Unity-Technologies/UnityCsReference/blob/3cfc6c4729d5cacedf67a38df5de1bfffb5994a3/Editor/Mono/ScriptAttributeGUI/PropertyHandler.cs
        private static PropertyDrawer GetDefaultDrawer(SerializedProperty property)
        {
            PropertyDrawer drawer = null;
            if (!DrawerDictionary.TryGetValue(property, out drawer))
            {
                Type propertyType = null;
                FieldInfo field = ScriptAttributeUtilityInternal.GetFieldInfoFromProperty(property, out propertyType);
                if (field != null)
                {
                    Type drawerType = ScriptAttributeUtilityInternal.GetDrawerTypeForType(propertyType);

                    // If we found a drawer type, instantiate the drawer, cache it, and return it.
                    if (drawerType != null)
                    {
                        if (typeof(PropertyDrawer).IsAssignableFrom(drawerType))
                        {
                            // Use PropertyDrawer on array elements, not on array itself.
                            // If there's a PropertyAttribute on an array, we want to apply it to the individual array elements instead.
                            // This is the only convenient way we can let the user apply PropertyDrawer attributes to elements inside an array.
                            if (propertyType != null && ScriptAttributeUtilityInternal.TypeIsArrayOrList(propertyType))
                                return null;

                            drawer = (PropertyDrawer)System.Activator.CreateInstance(drawerType);
                            PropertyDrawerFieldInfo.SetValue(drawer, field);
                        }
                    }
                }

                // Make a new one
                DrawerDictionary.Add(property, drawer);
            }

            return drawer;
        }
    }
}
