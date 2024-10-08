using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityInternalAccess.Editor;

public class SerializedPropertyTestObject : MonoBehaviour
{
    public enum TestEnum { A, B, C }

    [System.Serializable]
    public class MyClass
    {
        public int nestedInt;
    }
    
    public int myInt = 100;
    public string myString = "test";
    public TestEnum myEnum = TestEnum.A;
    public int[] myArray = { 0, 1, 2, 3 };
    public MyClass myClass;

    [ContextMenu(nameof(TestSerializedProperties))]
    private void TestSerializedProperties()
    {
        try
        {

            var serializedObject = new SerializedObject(this);

            var intProp = serializedObject.FindProperty(nameof(myInt));
            intProp.TryGetValue(out int intValue);
            Debug.Log($"{nameof(myInt)} {intProp.intValue} {intValue}");

            var stringProp = serializedObject.FindProperty(nameof(myString));
            stringProp.TryGetValue(out string stringValue);
            Debug.Log($"{nameof(myString)} {stringProp.stringValue} {stringValue}");

            var enumProp = serializedObject.FindProperty(nameof(myEnum));
            enumProp.TryGetValue(out TestEnum enumValue);
            Debug.Log($"{nameof(myEnum)} {(TestEnum)enumProp.intValue} {enumValue}");

            var arrayProp = serializedObject.FindProperty(nameof(myArray));
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var elementProp = arrayProp.GetArrayElementAtIndex(i);
                var parentProp = elementProp.GetParentProperty();

                elementProp.TryGetValue(out int elementValue);

                Debug.Log(
                    $"{nameof(arrayProp)}[{i}] {elementProp.intValue} {elementValue} (Parent matches: {arrayProp.propertyPath == parentProp.propertyPath})");
            }

            var classProp = serializedObject.FindProperty(nameof(myClass));
            var nestedIntProp = classProp.FindPropertyRelative(nameof(MyClass.nestedInt));
            classProp.TryGetValue(out MyClass classValue);
            nestedIntProp.TryGetValue(out int nestedIntValue);
            var nestedParentProp = nestedIntProp.GetParentProperty();
            Debug.Log(
                $"{nameof(classProp)} {nestedIntProp.intValue} {nestedIntValue} {classValue.nestedInt} (Parent matches: {classProp.propertyPath == nestedParentProp.propertyPath})");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
