using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [CustomPropertyDrawer(typeof(AngleRangeAttribute))]
    public class AngleRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Vector2 constraint = property.vector2Value;
            Vector2 limits = new Vector2(
                constraint.x - constraint.y,
                constraint.x + constraint.y);

            EditorGUI.Vector2Field(position, label, limits);

            constraint.x = (limits.x + limits.y) * 0.5f;
            constraint.y = limits.y - constraint.x;

            property.vector2Value = constraint;

            EditorGUI.EndProperty();
        }
    }
}