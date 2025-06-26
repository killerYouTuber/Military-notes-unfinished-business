using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]

public class MobileFV_Renamer : EditorWindow
{
    public string renameString = "";
    public string prefixString = "";
    [Range(0, 10)]
    int prefixNumber = 3;
    public string suffixString = "";
    int suffixNumber = 3;
    private bool whole = false;
    bool prefix = false;
    bool suffix = false;
    bool intIncrement = false;
    private string currentName;

    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_Renamer));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Renamer");

    }


    void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Change which parts of the name?", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        whole = EditorGUILayout.Toggle("Whole Name", whole);
        if (whole)
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            renameString = EditorGUILayout.TextField("Change Entire Name to:", renameString);
            intIncrement = EditorGUILayout.Toggle("Add numerical increment?", intIncrement);
            GUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        prefix = EditorGUILayout.Toggle("Prefix", prefix);
        if (prefix)
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            prefixNumber = (int)EditorGUILayout.Slider($"Remove first {prefixNumber}", prefixNumber, 0, 10);
            GUILayout.Label("Prepend the following:");
            prefixString = EditorGUILayout.TextField(prefixString);
            GUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        suffix = EditorGUILayout.Toggle("Suffix", suffix);
        if (suffix)
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));
            suffixNumber = (int)EditorGUILayout.Slider($"Remove last {suffixNumber}", suffixNumber, 0, 10);
            GUILayout.Label("Append the following:");
            suffixString = EditorGUILayout.TextField(suffixString);
            GUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        if (whole || prefix || suffix)
        {
            if (GUILayout.Button("Rename!", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true)))
            {
                var go = Selection.activeGameObject;
                if (go == null)
                    if (EditorUtility.DisplayDialog("Heads Up", "Can't rename meshes without something selected", "OK"))
                        return;

                RenameSelected();
            }
        }

        EditorGUILayout.EndVertical();
    }


    private void ChangeName(GameObject objToRename, int indexValue)
    {
        string newName = whole ? renameString : objToRename.name;
        // if we want to change the whole name
        if (whole)
        {
            string wholeName = "";
            if (intIncrement)
                wholeName = renameString + "_" + indexValue;
            else
                wholeName = renameString;

            newName = wholeName;
        }

        if (prefix)//change just the first characters
        {
            string preName = "";
            // store current name and remove the desired number of chars
            preName = newName.Remove(0, prefixNumber);
            preName = prefixString + preName;
            newName = preName;
        }

        if (suffix)// change the last characters
        {
            string sufName = "";
            sufName = newName.Remove(newName.Length - suffixNumber, suffixNumber);
            newName = sufName + suffixString;

        }

        objToRename.name = newName;
    }



    private void RenameSelected()
    {
        // run through everyone currently selected and change their name
        if (Selection.gameObjects.Length > 0)
            for (int i = 0; i < Selection.gameObjects.Length; i++)
                ChangeName(Selection.gameObjects[i], i);
        else
            ChangeName(Selection.activeGameObject, 0);
    }


}
#endif