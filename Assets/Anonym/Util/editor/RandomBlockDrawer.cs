using UnityEngine;
using UnityEditor;

namespace Anonym.Util
{
    [CustomPropertyDrawer(typeof(RandomBlock), true)]
    public class RandomBlockDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, "sdafasd");
            EditorGUI.EndProperty();
        }
    }
}