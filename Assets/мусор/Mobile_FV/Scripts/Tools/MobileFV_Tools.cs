using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

public class MobileFV_Tools : EditorWindow
{

    public static MobileFV_Tools instance;
    private int guiSpace = 20;

    public static void ShowEditor()
    {
        instance = (MobileFV_Tools)EditorWindow.GetWindow(typeof(MobileFV_Tools));
        instance.titleContent = new GUIContent("MobileFV Tools");
        instance.autoRepaintOnSceneChange = true;

    }

    private void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Tree Tools", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            MobileFV_TreeTools.ShowWindow();

        }
        GUILayout.Space(guiSpace);

        if (GUILayout.Button("Swap Material", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {

            MobileFV_SwapMat.ShowWindow();

        }
        GUILayout.Space(guiSpace);

        if (GUILayout.Button("Switch Mesh", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {

            MobileFV_SwitchMesh.ShowWindow();

        }
        GUILayout.Space(guiSpace);

        if (GUILayout.Button("Reset Transform", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {

            MobileFV_ResetTransform.ShowWindow();

        }
        GUILayout.Space(guiSpace);

        // ----------------- Collapse Mesh ----------------------

        if (GUILayout.Button("New Optimized", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            MobileFV_Collapse.ShowWindow();
        }
        GUILayout.Space(guiSpace);
        //-------------------- Save Prefab -----------------------


        if (GUILayout.Button("New Prefab", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {

            MobileFV_Prefabber.ShowWindow();
        }
        GUILayout.Space(guiSpace);


        //-------------------- Rename -----------------------
        if (GUILayout.Button("Renamer", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            MobileFV_Renamer.ShowWindow();

        }

        GUILayout.Space(guiSpace);

        if (GUILayout.Button("Screen Shot", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
        {
            MobileFV_Screenshots.ShowWindow();
        }

        GUILayout.EndVertical();

    }


}
#endif
