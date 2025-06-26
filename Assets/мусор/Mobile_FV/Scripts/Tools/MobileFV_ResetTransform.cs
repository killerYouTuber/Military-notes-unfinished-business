using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]
public class MobileFV_ResetTransform : EditorWindow
{

    private Vector3 _position_0 = new Vector3(0f, 0f, 0f);
    private Quaternion _rotation_0 = new Quaternion(0f, 0f, 0f, 0f);
    private Vector3 _scale_0 = new Vector3(1f, 1f, 1f);


    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_ResetTransform));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Reset Transform");

    }

    void OnGUI()
    {
        GUILayout.Label("Select Objects to Reset their transforms", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Transform on all selected", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                if (EditorUtility.DisplayDialog("Heads Up", "Can't reset transform without something selected", "OK"))
                    return;
            }
            ResetSelected();
        }


    }


    public void ResetSelected()
    {
        // run through all of the children of the current selection
        foreach (GameObject g in Selection.gameObjects)
        {

            Transform selMRa = g.transform.GetComponent<Transform>();
            if (selMRa != null)
            {
                selMRa.position = _position_0;
                selMRa.rotation = _rotation_0;
                selMRa.localScale = _scale_0;
            }

            for (int i = 0; i < g.transform.childCount; i++)
            {
                // if the child has a mesh renderer, swap it
                Transform selMRb = g.transform.GetChild(i).GetComponent<Transform>();
                if (selMRb != null)
                {
                    selMRb.position = _position_0;
                    selMRb.rotation = _rotation_0;
                    selMRb.localScale = _scale_0;
                }

                // if this child has children, perform the swap again
                if (g.transform.GetChild(i).transform.childCount > 0)
                {
                    for (int j = 0; j < g.transform.GetChild(i).transform.childCount; j++)
                    {
                        // if the child has a mesh renderer, swap it
                        Transform sel2MRc = g.transform.GetChild(i).transform.GetChild(j).GetComponent<Transform>();
                        if (sel2MRc != null)
                        {
                            sel2MRc.position = _position_0;
                            sel2MRc.rotation = _rotation_0;
                            sel2MRc.localScale = _scale_0;
                        }
                    }



                }


            }

            // if (EditorUtility.DisplayDialog("Mobile ForestVision Material Swap!", "Material Swap Successful on all Selected objects", "OK"))
            //     return;
        }
    }
}

#endif