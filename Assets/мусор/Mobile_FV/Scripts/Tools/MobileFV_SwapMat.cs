using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]
public class MobileFV_SwapMat : EditorWindow
{
    public Material swapMat = null;

    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_SwapMat));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Swap Material");
    }

    void OnGUI()
    {
        GUILayout.Label("What Material do you want to swap with?", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        swapMat = (Material)EditorGUILayout.ObjectField(swapMat, typeof(Material), true) as Material;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Swap Material on all selected", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            var go = Selection.activeGameObject;
            if (go == null)
                if (EditorUtility.DisplayDialog("Heads Up", "Can't swap materials without something selected", "OK"))
                    return;

            SwapMatOnSelected();
        }
    }

    private void SwapMat(GameObject thisGameObject)
    {
        // handle the current selection
        MeshRenderer mr = thisGameObject.transform.GetComponent<MeshRenderer>();
        // if this gameobject has a mesh renderer assigned, handle the swap
        if (mr != null)
            mr.material = swapMat;
    }

    public void LoopThroughChildren(GameObject currentGameObj)
    {
        // we know for certain we have children at this point, so loop through them
        // and decide if we need to switch out materials
        for (int i = 0; i < currentGameObj.transform.childCount; i++)
        {
            SwapMat(currentGameObj.transform.GetChild(i).gameObject);
            if (currentGameObj.transform.GetChild(i).transform.childCount > 0)
                LoopThroughChildren(currentGameObj.transform.GetChild(i).gameObject);

        }
    }

    public void SwapMatOnSelected()
    {
        // run through all of the children of the current selection
        foreach (GameObject g in Selection.gameObjects)
        {
            SwapMat(g);
            if (g.transform.childCount > 0)
                LoopThroughChildren(g);
        }
    }
}

#endif