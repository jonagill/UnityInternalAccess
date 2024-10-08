using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityInternalAccess.Editor
{
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Returns the C# value represented by the given SerializedProperty
        /// if it matches the requested type.
        /// </summary>
        public static bool TryGetValue<T>(this SerializedProperty property, out T value)
        {
            object untypedValue = property.GetValue();
            if (untypedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Returns the untyped C# value represented by the given SerializedProperty
        /// </summary>
        public static object GetValue(this SerializedProperty property)
        {
            if (property == null || property.serializedObject == null)
                return null;
            
            return GetPropertyValue(property.serializedObject.targetObject, property.propertyPath);
        }
        
        /// <summary>
        /// Returns the C# value at the given property path on the given object,
        /// if such a value exists and matches the requested type.
        /// </summary>
        public static bool TryGetPropertyValue<T>(object baseObject, string propertyPath, out T value)
        {
            object untypedValue = GetPropertyValue(baseObject, propertyPath);
            if (untypedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Returns the untyped C# value at the given property path on the given object,
        /// if such a value exists
        /// </summary>
        public static object GetPropertyValue(object baseObject, string propertyPath)
        {
            return GetPropertyValue(baseObject, propertyPath, 0);
        }

        private static object GetPropertyValue(object baseObject, string propertyPath, int elementsToIgnore)
        {
            var simplifiedPath = StripArrayTokensFromPropertyPath(propertyPath);
            var elements = simplifiedPath.Split('.');

            var obj = baseObject;
            foreach (var element in elements.Take(elements.Length - elementsToIgnore))
            {
                if (TryGetArrayNameAndIndexFromElement(element, out var arrayElement, out var arrayIndex))
                {
                    obj = GetArrayValueInternal(obj, arrayElement, arrayIndex);
                }
                else
                {
                    obj = GetValueInternal(obj, element);
                }
            }

            return obj;
        }

        /// <summary>
        /// Remove the verbose .Array.data path elements from a Unity serialized property path
        /// </summary>
        private static string StripArrayTokensFromPropertyPath(string propertyPath)
        {
            return propertyPath.Replace(".Array.data[", "[");
        }

        /// <summary>
        /// Adds the verbose .Array.data path elements to a previously stripped Unity serialized property path
        /// </summary>
        private static string AddArrayTokensToPropertyPath(string propertyPath)
        {
            return propertyPath.Replace(".Array.data[", "[");
        }
        
        /// <summary>
        /// Given a field or property element that is either the element name or the element name plus
        /// a bracketed index, return the element name and the index as separate values
        /// </summary>
        private static bool TryGetArrayNameAndIndexFromElement(string element, out string elementName, out int index)
        {
            if (element.Contains("["))
            {
                elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                index = Convert.ToInt32(
                    element.Substring(
                            element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "")
                        .Replace("]", ""));

                return true;
            }

            elementName = element;
            index = -1;
            return false;
        }

        private static object GetValueInternal(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();

            while (type != null)
            {
                var fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetValue(source);
                }

                // Handle properties as well as fields
                var propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(source, null);
                }

                type = type.BaseType;
            }
            return null;
        }

        private static object GetArrayValueInternal(object source, string name, int index)
        {
            var enumerable = GetValueInternal(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }

            return enumerator.Current;
        }
        
        /// <summary>
        /// Returns the SerializedProperty that contains this serialized property.
        /// For nested properties, returns the property that they are nested within.
        /// For array elements, returns the property of the array they are an element of.
        /// </summary>
        /// <param name="skipArrays">If true, will always return the property this property is nested within, essentially
        /// treating the path to array elements as the same as the path to the array itself.</param>
        public static SerializedProperty GetParentProperty(this SerializedProperty property, bool skipArrays = false)
        {
            if (property != null)
            {
                string parentPath = GetParentPropertyPath(property.propertyPath, skipArrays);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    return property.serializedObject.FindProperty(parentPath);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the path to the property that contains this property path.
        /// For nested properties, returns the path to the property that they are nested within.
        /// For array elements, returns the path to the property of the array they are an element of.
        /// </summary>
        /// <param name="skipArrays">If true, will always return the property this property is nested within, essentially
        /// treating the path to array elements as the same as the path to the array itself.</param>
        public static string GetParentPropertyPath(string propertyPath, bool skipArrays = false)
        {
            string strippedPropertyPath = StripArrayTokensFromPropertyPath(propertyPath);
            int lastIndexOfDot = strippedPropertyPath.LastIndexOf('.');
            int lastIndexOfArray = skipArrays ? -1 : strippedPropertyPath.LastIndexOf('[');
            int parentMarkerIndex = Mathf.Max(lastIndexOfArray, lastIndexOfDot);
            
            if (parentMarkerIndex > 0)
            {
                string strippedParentPropertyPath = strippedPropertyPath.Substring(0, parentMarkerIndex);
                return AddArrayTokensToPropertyPath(strippedParentPropertyPath);
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Returns the FieldInfo describing this SerializedProperty. For array and list elements,
        /// returns the FieldInfo describing the parent array or list.
        /// </summary>
        public static FieldInfo GetFieldInfo(this SerializedProperty serializedProperty, out Type fieldType)
        {
            return ScriptAttributeUtilityInternal.GetFieldInfoFromProperty( serializedProperty, out fieldType );
        }

        /// <summary>
        /// Returns the attributes on this SerializedProperty. For array and list elements,
        /// returns the attributes on the parent array or list.
        /// </summary>
        public static List<PropertyAttribute> GetFieldAttributes(this SerializedProperty serializedProperty)
        {
            var fieldInfo = GetFieldInfo( serializedProperty, out _ );
            return ScriptAttributeUtilityInternal.GetFieldAttributes( fieldInfo );
        }

        /// <summary>
        /// Returns the attributes of the specified type on this SerializedProperty. For array and list elements,
        /// returns the attributes on the parent array or list.
        /// </summary>
        public static IEnumerable<T> GetFieldAttributes<T>(
            this SerializedProperty serializedProperty) where T : PropertyAttribute
        {
            var fieldInfo = GetFieldInfo( serializedProperty, out _ );
            return ScriptAttributeUtilityInternal.GetFieldAttributes<T>( fieldInfo );
        }

        /// <summary>
        /// Returns the enum type of this SerializedProperty (or null if it is not an enum)
        /// </summary>
        public static Type GetEnumType(this SerializedProperty property)
        {
            if (property != null)
            {
                var fieldInfo = property.GetFieldInfo(out _);
                if (fieldInfo != null)
                {
                    return fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Returns the default value of the enum type that matches this SerializedProperty
        /// (or null if the property is not an enum)
        /// </summary>
        public static object GetDefaultEnumValue(this SerializedProperty property)
        {
            if (property != null)
            {
                var enumType = property.GetEnumType();
                if (enumType != null)
                {
                    var values = Enum.GetValues(enumType);
                    return values.GetValue(0);
                }
            }

            return null;
        }
    }
}

