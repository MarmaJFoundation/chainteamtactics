using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Anonym.Util;

[CustomPropertyDrawer(typeof(ArrayElementTitleAttribute))]
public class ArrayElementTitleDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                    GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    protected virtual ArrayElementTitleAttribute Atribute
    {
        get { return (ArrayElementTitleAttribute)attribute; }
    }
    SerializedProperty TitleNameProp;
    public override void OnGUI(Rect position,
                              SerializedProperty property,
                              GUIContent label)
    {
        string newlabel = "";
        
        if (Atribute.Type != null && typeof(IElementName).IsAssignableFrom(Atribute.Type))
        {
            int index = SerializedPropertyEX.GetIndex(property.propertyPath);
            var parentArray = SerializedPropertyEX.GetParentOfCurrent(property) as IList;

            if (parentArray != null && parentArray.Count > index && parentArray[index] != null)
                newlabel = (parentArray[index] as IElementName).GetElementName();
        }
        else
        {
            TitleNameProp = property.serializedObject.FindProperty(property.propertyPath + "." + Atribute.Varname);
            newlabel = GetTitle();
        }

        if (string.IsNullOrEmpty(newlabel))
            newlabel = label.text;
        EditorGUI.PropertyField(position, property, new GUIContent(newlabel, label.tooltip), true);
    }
    private string GetTitle()
    {
        switch (TitleNameProp.propertyType)
        {
            case SerializedPropertyType.Generic:
                break;
            case SerializedPropertyType.Integer:
                return TitleNameProp.intValue.ToString();
            case SerializedPropertyType.Boolean:
                return TitleNameProp.boolValue.ToString();
            case SerializedPropertyType.Float:
                return TitleNameProp.floatValue.ToString();
            case SerializedPropertyType.String:
                return TitleNameProp.stringValue;
            case SerializedPropertyType.Color:
                return TitleNameProp.colorValue.ToString();
            case SerializedPropertyType.ObjectReference:
                return TitleNameProp.objectReferenceValue.ToString();
            case SerializedPropertyType.LayerMask:
                break;
            case SerializedPropertyType.Enum:
                return TitleNameProp.enumNames[TitleNameProp.enumValueIndex];
            case SerializedPropertyType.Vector2:
                return TitleNameProp.vector2Value.ToString();
            case SerializedPropertyType.Vector3:
                return TitleNameProp.vector3Value.ToString();
            case SerializedPropertyType.Vector4:
                return TitleNameProp.vector4Value.ToString();
            case SerializedPropertyType.Rect:
                break;
            case SerializedPropertyType.ArraySize:
                break;
            case SerializedPropertyType.Character:
                break;
            case SerializedPropertyType.AnimationCurve:
                break;
            case SerializedPropertyType.Bounds:
                break;
            case SerializedPropertyType.Gradient:
                break;
            case SerializedPropertyType.Quaternion:
                break;
            default:                
                break;
        }
        return "";
    }
}