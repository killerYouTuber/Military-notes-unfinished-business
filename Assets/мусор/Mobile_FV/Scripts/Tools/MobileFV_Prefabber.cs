using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
public class MobileFV_Prefabber : EditorWindow
{
    private bool changeName = false;
    private bool setPath = false;

    private string newName = System.String.Empty;
    private string tempName = "new";

    private string prefabPath = "Assets/Mobile_FV/Prefabs";

    string placeholder = string.Empty;


    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_Prefabber));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Save New Prefab");
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("Save Prefab", EditorStyles.boldLabel);
        if (Selection.gameObjects.Length > 1)
        {
            EditorGUILayout.LabelField("Select only one object", EditorStyles.label);
        }
        else if (Selection.activeGameObject)
        {
            tempName = Selection.activeGameObject.name;
            EditorGUILayout.LabelField(tempName, EditorStyles.miniLabel);
        }
        else
        {
            EditorGUILayout.LabelField("Select something to see proposed name...", EditorStyles.miniLabel);
            tempName = newName = string.Empty;
        }
        EditorGUILayout.Space();



        if (Selection.gameObjects.Length == 1)
        {
            placeholder = Selection.activeGameObject.name;

            changeName = EditorGUILayout.Toggle("Change prefab name?", changeName);

            if (changeName)
            {

                GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));

                newName = EditorGUILayout.TextField("Change Entire Name to:", newName == string.Empty ? placeholder : newName);
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            else
                newName = tempName;

            EditorGUILayout.LabelField(prefabPath, EditorStyles.miniLabel);
            setPath = EditorGUILayout.Toggle("Set a Specific Path?", setPath);

            if (setPath)
            {

                GUILayout.Label("Where do you want the new prefab?", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(prefabPath, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                    prefabPath = GetRelativePath(EditorUtility.SaveFolderPanel("Path to Save Prefab", prefabPath, Application.persistentDataPath));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            else
            {
                prefabPath = "Assets/Mobile_FV/Prefabs";// reset the path
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Save New Prefab!", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true)))
                SavePrefab();
            // if (SavePrefab())// if save was successful
            // {
            //     newName = string.Empty;
            //     tempName = string.Empty;
            // }
            // else
            // {//save didnt happen
            //     Debug.Log("Save didn't go through");
            // }

        }

        EditorGUILayout.EndVertical();
    }

    private string GetRelativePath(string rawPath)
    {
        // if user wants to set their own path, we need to adjust for relative path to avoid warning
        string[] splitPath = rawPath.Split(new string[] { "/Assets" }, System.StringSplitOptions.None);
        return "Assets" + splitPath[1];

    }


    private void SavePrefab()
    {

        GameObject selected = Selection.activeGameObject;//Instantiate(Selection.activeGameObject, Vector3.zero, Quaternion.identity);
                                                         // ResetSelectedTransform(selected);


        if (changeName)
            prefabPath = prefabPath + "/" + newName + ".prefab";
        else
            prefabPath = prefabPath + "/" + selected.name + ".prefab";

        PrefabUtility.SaveAsPrefabAsset(selected, prefabPath);

        // if (successObject)
        // {
        //     // DestroyImmediate(successObject);
        //     // DestroyImmediate(selected);
        //     return true;
        // }
        // else
        // {
        //     // DestroyImmediate(selected);
        //     return false;
        // }


    }


    private void ResetSelectedTransform(GameObject selectedObject)
    {
        selectedObject.transform.localPosition = Vector3.zero;
        selectedObject.transform.localEulerAngles = Vector3.zero;
        selectedObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }



}

#endif