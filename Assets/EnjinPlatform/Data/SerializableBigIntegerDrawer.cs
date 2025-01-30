using System.Numerics;
using UnityEditor;
using UnityEngine;

namespace EnjinPlatform.Data
{
// TODO: DOCS - Explain why this is needed to support the SerializableBigInteger class in the Unity Editor.
    [CustomPropertyDrawer(typeof(SerializableBigInteger))]
    public class SerializableBigIntegerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the string value from the property
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            string value = valueProperty.stringValue;

            // Convert the string to BigInteger for display
            BigInteger bigIntValue;
            BigInteger.TryParse(value, out bigIntValue);

            // Display the BigInteger as a string field
            string newValue = EditorGUI.TextField(position, label, bigIntValue.ToString());

            // Update the BigInteger value if it has changed
            if (newValue != bigIntValue.ToString())
            {
                valueProperty.stringValue = newValue;
            }

            EditorGUI.EndProperty();
        }
    }
}