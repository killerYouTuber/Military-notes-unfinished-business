using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
public class MobileFV_Collapse : EditorWindow
{
    private bool changeName = false;
    private bool setPath = false;

    private string newName = System.String.Empty;
    private string tempName = "_optimized";
    private List<GameObject> childrenOfTree = new List<GameObject>();
    private string collapsedPath = "Assets/Mobile_FV/Optimized/_CollapsedMeshes";
    private string optimizedPath = "Assets/Mobile_FV/Optimized";
    string placeholder = string.Empty;


    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_Collapse));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Collapse Selected Mesh");
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("Collapse Mesh", EditorStyles.boldLabel);
        if (Selection.gameObjects.Length > 1)
        {
            EditorGUILayout.LabelField("Select only one object", EditorStyles.label);
        }
        else if (Selection.activeGameObject)
        {
            tempName = $"Opt_{Selection.activeGameObject.name}";
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
            placeholder = $"Opt_{Selection.activeGameObject.name}";
            EditorGUILayout.LabelField($"Total Vertex Count on Optimized Asset: {GetTotalVertexCount(Selection.activeGameObject).ToString()}", EditorStyles.boldLabel);

            changeName = EditorGUILayout.Toggle("Change optimized name?", changeName);

            if (changeName)
            {

                GUILayout.BeginVertical("box", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true));

                newName = EditorGUILayout.TextField("Change Entire Name to:", newName == string.Empty ? placeholder : newName);
                GUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            else
                newName = tempName;

            EditorGUILayout.LabelField(optimizedPath, EditorStyles.miniLabel);
            setPath = EditorGUILayout.Toggle("Set a Specific Path?", setPath);

            if (setPath)
            {

                GUILayout.Label("Where do you want the Optimized tree?", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(optimizedPath, GUILayout.ExpandWidth(false));
                if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                    optimizedPath = GetRelativePath(EditorUtility.SaveFolderPanel("Path to Save Tree", optimizedPath, Application.persistentDataPath));
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space();
            }
            else
            {
                optimizedPath = "Assets/Mobile_FV/Optimized";// reset the path
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Collapse to single mesh!", GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true)))
                if (CombineMeshes())// if save was successful
                {
                    newName = string.Empty;
                    tempName = string.Empty;
                }
                else
                {//save didnt happen
                    Debug.Log("Save didn't go through");
                }

        }


        EditorGUILayout.EndVertical();
    }

    private string GetRelativePath(string rawPath)
    {
        // if user wants to set their own path, we need to adjust for relative path to avoid warning
        string[] splitPath = rawPath.Split(new string[] { "/Assets" }, System.StringSplitOptions.None);
        return "Assets" + splitPath[1];

    }

    private int GetTotalVertexCount(GameObject selectedObject)
    {
        MeshRenderer[] meshRenderers = selectedObject.GetComponentsInChildren<MeshRenderer>(false);
        int totalVertexCount = 0;
        int totalMeshCount = 0;

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    totalVertexCount += filter.sharedMesh.vertexCount;
                    totalMeshCount++;
                }
            }
        }
        return totalVertexCount;
    }

    private bool CombineMeshes()
    {

        GameObject selected = Selection.activeGameObject;

        MeshRenderer[] meshRenderers = selected.GetComponentsInChildren<MeshRenderer>(false);
        int totalVertexCount = 0;
        int totalMeshCount = 0;

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    totalVertexCount += filter.sharedMesh.vertexCount;
                    totalMeshCount++;
                }
            }
        }

        if (totalMeshCount == 0)
        {
            Debug.Log("No meshes found in children. There's nothing to combine.");

        }
        if (totalMeshCount == 1)
        {
            Debug.Log("Only 1 mesh found in children. There's nothing to combine.");

        }
        if (totalVertexCount > 65535)
        {
            Debug.Log("There are too many vertices to combine into 1 mesh (" + totalVertexCount + "). The max. limit is 65535");

        }

        Mesh mesh = new Mesh();
        Matrix4x4 myTransform = selected.transform.worldToLocalMatrix;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv1s = new List<Vector2>();
        List<Vector2> uv2s = new List<Vector2>();
        Dictionary<Material, List<int>> subMeshes = new Dictionary<Material, List<int>>();

        if (meshRenderers != null && meshRenderers.Length > 0)
        {

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    MergeMeshInto(filter.sharedMesh, meshRenderer.sharedMaterials, myTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s, subMeshes);
                    //add each child to the children list to delete at the very end
                    if (filter.gameObject != selected)
                        childrenOfTree.Add(filter.gameObject);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0)
            mesh.normals = normals.ToArray();
        if (uv1s.Count > 0)
            mesh.uv = uv1s.ToArray();

        mesh.subMeshCount = subMeshes.Keys.Count;
        Material[] materials = new Material[subMeshes.Keys.Count];
        int mIdx = 0;

        foreach (Material m in subMeshes.Keys)
        {
            materials[mIdx] = m;
            mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
        }

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            MeshRenderer meshRend = selected.GetComponent<MeshRenderer>();
            if (meshRend == null)
                meshRend = selected.AddComponent<MeshRenderer>();
            meshRend.sharedMaterials = materials;

            MeshFilter meshFilter = selected.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = selected.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            //Store new combined mesh asset
            Mesh _mesh = new Mesh();
            string path = "";
            if (changeName)
                path = collapsedPath + "/" + newName + ".asset";
            else
                path = collapsedPath + "/" + selected.name + ".asset";

            AssetDatabase.CreateAsset(selected.GetComponent<MeshFilter>().sharedMesh, path);

            /* handle new prefab*/

            // dig out the mesh asset we just saved 
            _mesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, typeof(Mesh));

            // create a freah new game object
            GameObject newMeshToSave = new GameObject("new");
            // set it up
            newMeshToSave.AddComponent<MeshFilter>();
            newMeshToSave.GetComponent<MeshFilter>().sharedMesh = _mesh;
            newMeshToSave.AddComponent<MeshRenderer>();
            newMeshToSave.GetComponent<MeshRenderer>().sharedMaterial = selected.GetComponent<MeshRenderer>().sharedMaterial;

            /* Begin to set up the LOD aspect */

            // create the empty parent
            GameObject go = new GameObject("Parent");
            newMeshToSave.transform.parent = go.transform;

            // reset the transform completely 
            ResetSelectedTransform(newMeshToSave);

            // add LOD
            go.AddComponent<LODGroup>();
            MeshRenderer[] newMR = go.GetComponentsInChildren<MeshRenderer>();
            // set the initial cull to 10%
            LOD[] initialLOD = new LOD[] { new LOD(10f / 100f, newMR) };
            go.GetComponent<LODGroup>().SetLODs(initialLOD);

            if (changeName)
            {
                go.name = newName;
                newMeshToSave.name = $"{newName}_coreMesh_LOD0";
            }
            else
            {
                go.name = tempName;
                newMeshToSave.name = $"{tempName}_coreMesh_LOD0";
            }



            string prefabPath = "";
            if (changeName)
                prefabPath = optimizedPath + "/" + newName + ".prefab";
            else
                prefabPath = optimizedPath + "/" + go.name + ".prefab";

            GameObject successObject = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Selection.activeGameObject = selected;
            DestroyImmediate(go);

            return (successObject != null);


        }

        return false;
    }


    private void ResetSelectedTransform(GameObject selectedObject)
    {
        selectedObject.transform.localPosition = Vector3.zero;
        selectedObject.transform.localEulerAngles = Vector3.zero;
        selectedObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }


    private void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes)
    {
        if (meshToMerge == null)
            return;
        int vertexOffset = vertices.Count;
        Vector3[] vs = meshToMerge.vertices;

        for (int i = 0; i < vs.Length; i++)
        {
            vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);
        }
        vertices.AddRange(vs);

        Quaternion rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
        Vector3[] ns = meshToMerge.normals;
        if (ns != null && ns.Length > 0)
        {
            for (int i = 0; i < ns.Length; i++)
                ns[i] = rotation * ns[i];
            normals.AddRange(ns);
        }

        Vector2[] uvs = meshToMerge.uv;
        if (uvs != null && uvs.Length > 0)
            uv1s.AddRange(uvs);
        uvs = meshToMerge.uv2;
        if (uvs != null && uvs.Length > 0)
            uv2s.AddRange(uvs);

        for (int i = 0; i < ms.Length; i++)
        {
            if (i < meshToMerge.subMeshCount)
            {
                int[] ts = meshToMerge.GetTriangles(i);
                if (ts.Length > 0)
                {
                    if (ms[i] != null && !subMeshes.ContainsKey(ms[i]))
                    {
                        subMeshes.Add(ms[i], new List<int>());
                    }
                    List<int> subMesh = subMeshes[ms[i]];
                    for (int t = 0; t < ts.Length; t++)
                    {
                        ts[t] += vertexOffset;
                    }
                    subMesh.AddRange(ts);
                }
            }
        }
    }
}

#endif