using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponParametrs))]
public class WeaponParametrsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "m_Script")
            {
                continue;
            }

            if (property.name == "isShotgun")
            {
                EditorGUILayout.PropertyField(property, true);
                if (property.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pelletCount"), true);
                }
            }
            else if (property.name == "pelletCount")
            {
                // Already handled above
            }
            else
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        serializedObject.ApplyModifiedProperties();

        // --- Aiming Helper ---
        WeaponParametrs weapon = (WeaponParametrs)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Aiming Position Helper", EditorStyles.boldLabel);

        if (GUILayout.Button("Copy Current Transform to Aim"))
        {
            if (Application.isPlaying)
            {
                Undo.RecordObject(weapon, "Copy Transform to Aim");
                weapon.aimPosition = weapon.transform.localPosition;
                weapon.aimRotation = weapon.transform.localRotation.eulerAngles;
                EditorUtility.SetDirty(weapon);
                Debug.Log($"Copied Local Position: {weapon.aimPosition} and Rotation: {weapon.aimRotation} to Aim Settings.");
            }
            else
            {
                Debug.LogWarning("This feature only works in Play Mode. Adjust the weapon's position while playing, then press this button.");
            }
        }
        EditorGUILayout.HelpBox("To set the aiming position:\n1. Enter Play Mode.\n2. Select the weapon instance in the hierarchy.\n3. Manually adjust its Local Position and Rotation.\n4. Click 'Copy Current Transform to Aim'.\n5. Exit Play Mode. The values will be saved.", MessageType.Info);
    }
} 