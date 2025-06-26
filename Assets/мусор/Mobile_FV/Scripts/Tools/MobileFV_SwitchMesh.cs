using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]

public class MobileFV_SwitchMesh : EditorWindow
{

    int meshVersion = 1;
    int seasonalVersion = 1;


    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_SwitchMesh));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Switch Mesh");

    }


    void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Switch Meshes on Selected", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (Selection.activeGameObject)
        {
            MeshFilter selMf = Selection.activeGameObject.transform.GetComponent<MeshFilter>();
            if (selMf)
            {

                // verify that we are selected on A FV Mesh before running any of the following methods
                if (!isMFVMesh())
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Not a Mobile ForestVision Mesh. Please select a MFV mesh", EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    // valid MFV mesh, handle its options
                    int initialSeasonalVersion = int.Parse(selMf.sharedMesh.name.Substring(1).First().ToString());
                    seasonalVersion = initialSeasonalVersion;
                    seasonalVersion = EditorGUILayout.IntSlider("Seasonal Number", seasonalVersion, 1, 4);
                    SwitchSeasonalVersion(seasonalVersion);

                    int initialMeshVersion = int.Parse(selMf.sharedMesh.name.Last().ToString());
                    meshVersion = initialMeshVersion;
                    meshVersion = EditorGUILayout.IntSlider("Variety Number", meshVersion, 1, 3);
                    SwitchVersion(meshVersion);
                }
            }
            else
            {
                GUILayout.Label("Select a MFV mesh to see your options", EditorStyles.wordWrappedLabel);
            }

        }
    }

    #region Mesh Checks
    private bool isMFVMesh()
    {
        return (GetFoliageType("C1_") || GetFoliageType("C2_") || GetFoliageType("C3_") || GetFoliageType("C4_"));
    }

    private bool isMFVTree()
    {
        return (GetTreeType("C1__Tree") || GetTreeType("C2__Tree") || GetTreeType("C3__Tree") || GetTreeType("C4__Tree"));
    }


    ///<summary>Check to see if the passed string matches what the meshfilter mesh name starts with. </summary>
    private bool GetFoliageType(string nameCompare)
    {
        return Selection.activeGameObject.transform.GetComponent<MeshFilter>().sharedMesh.name.StartsWith(nameCompare);
    }

    private bool GetTreeType(string nameCompare)
    {
        return Selection.activeGameObject.name.StartsWith(nameCompare);
    }

    private bool GetFoliageTypeOfParent(string nameCompare)
    {
        return Selection.activeGameObject.transform.parent.transform.GetComponent<MeshFilter>().sharedMesh.name.StartsWith(nameCompare);
    }


    #endregion




    private void SwitchMesh(GameObject thisGameObject, Mesh[] fbxMeshes, int version)
    {
        // handle the current selection
        MeshFilter mf = thisGameObject.transform.GetComponent<MeshFilter>();
        string oldMeshName;
        string newMeshName;
        // if this gameobject has a mesh filter assigned, handle the swap
        if (mf != null)
        {
            // store the name of this current mesh
            oldMeshName = mf.sharedMesh.name;
            // remove the last character from the name, ie, the version number...add on the version number we want to switch to
            newMeshName = mf.sharedMesh.name.Remove(mf.sharedMesh.name.Length - 1) + version.ToString();

            if (oldMeshName != newMeshName) // as long as the swap mesh is different, swap it
                foreach (Mesh mesh in fbxMeshes)// run through and find the source mesh we want to switch with
                    if (mesh.name == newMeshName)// if we find the name of what we want to swap with in the fbx file
                        mf.sharedMesh = mesh;// swap meshes
        }
    }

    private void SwitchCombinedMesh(GameObject thisGameObject, Mesh[] fbxMeshes, int version)
    {
        // handle the current selection
        MeshFilter mf = thisGameObject.transform.GetComponent<MeshFilter>();
        string oldMeshName;
        string newMeshName;
        // if this gameobject has a mesh filter assigned, handle the swap
        if (mf != null)
        {
            // store the name of this current mesh
            oldMeshName = mf.sharedMesh.name;
            // remove first 2 characters
            var tempName = mf.sharedMesh.name.Remove(0, 2);
            // add the updated prefix
            string prefix = "C" + version.ToString();
            newMeshName = prefix + tempName;

            if (oldMeshName != newMeshName) // as long as the swap mesh is different, swap it
                foreach (Mesh mesh in fbxMeshes)// run through and find the source mesh we want to switch with
                    if (mesh.name == newMeshName)// if we find the name of what we want to swap with in the fbx file
                        mf.sharedMesh = mesh;// swap meshes
        }
    }




    private int GetTreeTypeIndex(GameObject selectedTree)
    {
        switch (selectedTree.gameObject.name)
        {
            case "C1__Tree_5Arms": return 1;
            case "C1__Tree_ConiferShort": return 2;
            case "C1__Tree_ConiferTall": return 3;
            case "C1__Tree_ConiferYoung": return 4;
            case "C1__Tree_FullTree": return 5;
            case "C1__Tree_short": return 6;
            case "C1__Tree_single": return 7;
            case "C1__Tree_SkinnyBunches": return 8;
            case "C1__Tree_SkinnyTall": return 9;
            case "C1__Tree_TallThin": return 10;
            case "C1__Tree_young": return 11;
            case "C1__TreeCurved": return 12;
            default: return -1;
        }
    }

    private string GetTreeTypeName(int typeVersion)
    {
        switch (typeVersion)
        {
            case 1: return "C1__Tree_5Arms";
            case 2: return "C1__Tree_ConiferShort";
            case 3: return "C1__Tree_ConiferTall";
            case 4: return "C1__Tree_ConiferYoung";
            case 5: return "C1__Tree_FullTree";
            case 6: return "C1__Tree_short";
            case 7: return "C1__Tree_single";
            case 8: return "C1__Tree_SkinnyBunches";
            case 9: return "C1__Tree_SkinnyTall";
            case 10: return "C1__Tree_TallThin";
            case 11: return "C1__Tree_young";
            case 12: return "C1__TreeCurved";
            default: return "unknown";
        }
    }
    public void SwitchVersion(int version)
    {
        // load the file once, and then pass it down
        Mesh[] fbxMeshes = Resources.LoadAll<Mesh>("Combined4Meshes");
        // run through all of the children of the current selection
        foreach (GameObject g in Selection.gameObjects)
        {
            SwitchMesh(g, fbxMeshes, version);

            // and now figure out if we need to switch out children of our selection
            if (g.transform.childCount > 0)
                LoopThroughChildren(g, fbxMeshes, version);
        }
    }

    public void SwitchSeasonalVersion(int version)
    {
        // load the file once, and then pass it down
        Mesh[] fbxMeshes = Resources.LoadAll<Mesh>("Combined4Meshes");
        // run through all of the children of the current selection
        foreach (GameObject g in Selection.gameObjects)
        {
            //switch out our current selection
            SwitchCombinedMesh(g, fbxMeshes, version);

            // and now figure out if we need to switch out children of our selection
            if (g.transform.childCount > 0)
                LoopThroughCombinedChildren(g, fbxMeshes, version);
        }
    }


    public void LoopThroughChildren(GameObject currentGameObj, Mesh[] fbxMeshes, int version)
    {
        // we know for certain we have children at this point, so loop through them
        // and decide if we need to switch out meshes
        for (int i = 0; i < currentGameObj.transform.childCount; i++)
        {
            SwitchMesh(currentGameObj.transform.GetChild(i).gameObject, fbxMeshes, version);

            if (currentGameObj.transform.GetChild(i).transform.childCount > 0)
            {
                LoopThroughChildren(currentGameObj.transform.GetChild(i).gameObject, fbxMeshes, version);
            }
        }
    }

    public void LoopThroughCombinedChildren(GameObject currentGameObj, Mesh[] fbxMeshes, int version)
    {
        // we know for certain we have children at this point, so loop through them
        // and decide if we need to switch out meshes
        for (int i = 0; i < currentGameObj.transform.childCount; i++)
        {
            SwitchCombinedMesh(currentGameObj.transform.GetChild(i).gameObject, fbxMeshes, version);

            if (currentGameObj.transform.GetChild(i).transform.childCount > 0)
            {
                LoopThroughCombinedChildren(currentGameObj.transform.GetChild(i).gameObject, fbxMeshes, version);
            }
        }
    }


}
#endif