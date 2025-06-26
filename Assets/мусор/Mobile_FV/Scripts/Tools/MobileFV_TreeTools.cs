using UnityEngine;

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

public class MobileFV_TreeTools : EditorWindow
{
    float xRotation, yRotation, zRotation, scaleFactor;
    float originalX, originalY, originalZ; //used to get original rotation values of branch
    Vector3 originalBranchRotation; //used to apply original values back to the branch

    List<GameObject> spawners = new List<GameObject>(); // The list of objects to be spawn
    int count = 5;                      // The amount of times to spawn objects
    bool ofEach = false;                // Tell if each needs to be spawned by the 'count'
    Vector3 scaleUp = Vector3.one;      // The static start scale of the object
                                        //Vector3 maxScaleUp = Vector3.one; 
    float scalingValue;
    GameObject newSpawner = null;       // The template for adding objects
    bool justSelected = false;
    bool justScaleSelected = false;
    bool localSpace = false;
    float spawnedXRotation, spawnedYRotation, spawnedZRotation;
    int randomAmount;
    int setRotation;

    //	bool spawnX = true;
    //	bool spawnY = true;
    //	bool spawnZ = true;
    bool spawnUpwardsX = true;
    bool spawnUpwardsY = true;
    bool spawnUpwardsZ = true;
    bool deleteOld = true;


    float minHeight = 5f;
    Vector2 windowScroll = Vector2.zero;

    public enum rotAxis
    {
        x,
        y,
        z
    }
    rotAxis curAxis = rotAxis.y;

    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_TreeTools));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Tree Tools");
        editorWindow.maxSize = new Vector2(420f, 750f);
        editorWindow.minSize = editorWindow.maxSize;
    }
    void DeleteOldBranches(GameObject obj)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            if (obj.transform.GetChild(i).name.EndsWith("FVbranch"))
            {
                GameObject.DestroyImmediate(obj.transform.GetChild(i).gameObject);
                i--;
            }
        }
    }

    void OnGUI()
    {
        GUILayout.ExpandWidth(false);
        ;
        windowScroll = EditorGUILayout.BeginScrollView(windowScroll);

        GUILayout.Label("Tree Customizer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.Space();
        ///////////////////////////////////////////////////////////////////////////////////////

        EditorGUILayout.LabelField("Branch Spawner", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        newSpawner = EditorGUILayout.ObjectField("Load Branch Type here:", null, typeof(GameObject), true) as GameObject;

        if (newSpawner != null)
            spawners.Add(newSpawner);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(spawners.Count > 0 ? "Branch List:" : "");

        for (int i = 0; i < spawners.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            spawners[i] = EditorGUILayout.ObjectField(spawners[i], typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("Delete"))
            {
                spawners.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();


        if (spawners.Count > 0)
        {
            count = (int)EditorGUILayout.Slider("How Many?", count, 1, 5);
            ofEach = EditorGUILayout.Toggle("Per Branch Type:", ofEach);
            scalingValue = 0.9f;
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pre-spawn Adjustments", EditorStyles.boldLabel);
        //if (GUILayout.Button ("Reset All Settings")) {
        //ResetAllSettingsBeforeSpawn();
        //break;
        //}

        EditorGUILayout.EndHorizontal();


        //minHeight = EditorGUILayout.Slider ("Starting Height", minHeight, 0, MaxHeightFromCollider (Selection.activeGameObject));

        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 205;
        deleteOld = EditorGUILayout.Toggle("Delete Old Branches on Generate?", deleteOld, GUILayout.ExpandWidth(false));

        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = 100;
        minHeight = EditorGUILayout.Slider("Starting Height", minHeight, 0, 500);

        EditorGUIUtility.labelWidth = 100;
        setRotation = (int)EditorGUILayout.Slider("Set Rotation", setRotation, 0, 180);

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 220;

        EditorGUILayout.Space();


        EditorGUILayout.BeginHorizontal();

        EditorGUIUtility.labelWidth = 45;

        spawnUpwardsX = EditorGUILayout.Toggle("Up X", spawnUpwardsX);
        spawnUpwardsY = EditorGUILayout.Toggle("Up Y", spawnUpwardsY);
        spawnUpwardsZ = EditorGUILayout.Toggle("Up Z", spawnUpwardsZ);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 95;
        EditorGUIUtility.fieldWidth = 20;

        if (GUILayout.Button("Gen X", GUILayout.MinHeight(40)))
        {

            if (spawners.Count == 0)
            {
                Debug.LogError("No branches have been set. Please Load Branches into the Spawner first.");
            }
            else
            {
                foreach (GameObject s in spawners)
                    GenerateBranchesX(Selection.activeGameObject, s);
            }


        }

        if (GUILayout.Button("Gen Y", GUILayout.MinHeight(40)))
        {

            if (spawners.Count == 0)
            {
                Debug.LogError("No branches have been set. Please Load Branches into the Spawner first.");
            }
            else
            {

                foreach (GameObject s in spawners)
                    GenerateBranchesY(Selection.activeGameObject, s);
            }


        }

        if (GUILayout.Button("Gen Z", GUILayout.MinHeight(40)))
        {

            if (spawners.Count == 0)
            {
                Debug.LogError("No branches have been set. Please Load Branches into the Spawner first.");
            }
            else
            {

                foreach (GameObject s in spawners)
                    GenerateBranchesZ(Selection.activeGameObject, s);
            }


        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate on All Axis", GUILayout.MinHeight(40)))
        {
            //Undo.RegisterSceneUndo ("Hold Undo");
            if (spawners.Count == 0)
            {
                Debug.LogError("No branches have been set. Please Load Branches into the Spawner first.");
            }
            else
            {

                MakeBranches(Selection.activeGameObject);
            }


        }

        EditorGUILayout.Space();
        randomAmount = (int)EditorGUILayout.Slider("RandomAmount", randomAmount, 0, 90);


        EditorGUIUtility.labelWidth = 95;
        EditorGUIUtility.fieldWidth = 20;
        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button("Randomize Branches", GUILayout.MinHeight(40)))
        {
            RandomizeAllBranches();
        }

        if (GUILayout.Button("Clear Branches", GUILayout.MinHeight(40)))
        {
            DeleteOldBranches(Selection.activeGameObject);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();


        EditorGUILayout.BeginVertical("box");
        EditorGUIUtility.labelWidth = 130;
        EditorGUILayout.LabelField("Rotate All Branches on the following axis:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        curAxis = (rotAxis)EditorGUILayout.EnumPopup("Rotation Axis:", curAxis);
        justSelected = EditorGUILayout.Toggle("Rotate Just Selected", justSelected);
        localSpace = EditorGUILayout.Toggle("Rotate in Local", localSpace);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+1"))
        {
            if (justSelected)
                RotateChildren(1, false, curAxis, localSpace);

            RotateChildren(1, true, curAxis, localSpace);
        }
        if (GUILayout.Button("+5"))
        {
            if (justSelected)
                RotateChildren(5, false, curAxis, localSpace);

            RotateChildren(5, true, curAxis, localSpace);
        }
        if (GUILayout.Button("+10"))
        {
            if (justSelected)
                RotateChildren(10, false, curAxis, localSpace);

            RotateChildren(10, true, curAxis, localSpace);
        }
        if (GUILayout.Button("+45"))
        {
            if (justSelected)
                RotateChildren(45, false, curAxis, localSpace);

            RotateChildren(45, true, curAxis, localSpace);
        }
        if (GUILayout.Button("+90"))
        {
            if (justSelected)
                RotateChildren(90, false, curAxis, localSpace);

            RotateChildren(90, true, curAxis, localSpace);
        }
        if (GUILayout.Button("+180"))
        {
            if (justSelected)
                RotateChildren(180, false, curAxis, localSpace);

            RotateChildren(180, true, curAxis, localSpace);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-1"))
        {
            if (justSelected)
                RotateChildren(-1, false, curAxis, localSpace);

            RotateChildren(-1, true, curAxis, localSpace);
        }
        if (GUILayout.Button("-5"))
        {
            if (justSelected)
                RotateChildren(-5, false, curAxis, localSpace);

            RotateChildren(-5, true, curAxis, localSpace);
        }
        if (GUILayout.Button("-10"))
        {
            if (justSelected)
                RotateChildren(-10, false, curAxis, localSpace);

            RotateChildren(-10, true, curAxis, localSpace);
        }
        if (GUILayout.Button("-45"))
        {
            if (justSelected)
                RotateChildren(-45, false, curAxis, localSpace);

            RotateChildren(-45, true, curAxis, localSpace);
        }
        if (GUILayout.Button("-90"))
        {
            if (justSelected)
                RotateChildren(-90, false, curAxis, localSpace);

            RotateChildren(-90, true, curAxis, localSpace);
        }
        if (GUILayout.Button("-180"))
        {
            if (justSelected)
                RotateChildren(-180, false, curAxis, localSpace);

            RotateChildren(-180, true, curAxis, localSpace);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 130;
        EditorGUILayout.LabelField("Scale All Branches Uniformly:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        justScaleSelected = EditorGUILayout.Toggle("Scale Just Selected", justScaleSelected);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("0.25"))
        {
            if (justScaleSelected)
                ScaleAllBranches(0.25f, false);

            ScaleAllBranches(0.25f, true);
        }
        if (GUILayout.Button("0.5"))
        {
            if (justScaleSelected)
                ScaleAllBranches(0.5f, false);

            ScaleAllBranches(0.5f, true);
        }
        if (GUILayout.Button("0.9"))
        {
            if (justScaleSelected)
                ScaleAllBranches(0.9f, false);

            ScaleAllBranches(0.9f, true);
        }
        if (GUILayout.Button("1.1"))
        {
            if (justScaleSelected)
                ScaleAllBranches(1.1f, false);

            ScaleAllBranches(1.1f, true);
        }
        if (GUILayout.Button("1.25"))
        {
            if (justScaleSelected)
                ScaleAllBranches(1.25f, false);

            ScaleAllBranches(1.25f, true);
        }
        if (GUILayout.Button("1.5"))
        {
            if (justScaleSelected)
                ScaleAllBranches(1.5f, false);

            ScaleAllBranches(1.5f, true);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        ///////////////////////////////////////////////////////////////////////////////////////
        //EditorGUILayout.BeginVertical ("box");
        //EditorGUILayout.Space ();

        //EditorGUILayout.LabelField ("Tree Painter", EditorStyles.boldLabel);
        //vertexSpawnBranch = EditorGUILayout.ObjectField(vertexSpawnBranch, typeof(GameObject), true);

        //EditorGUILayout.EndVertical ();
        EditorGUILayout.EndScrollView();

    }



    void RandomizeAllBranches()
    {
        foreach (GameObject g in Selection.gameObjects)
            for (int i = 0; i < g.transform.childCount; i++)
                //if (g.transform.GetChild(i).name.EndsWith("FVbranch"))
                g.transform.GetChild(i).Rotate(Random.Range(-randomAmount, randomAmount), Random.Range(-randomAmount, randomAmount), Random.Range(-randomAmount, randomAmount), Space.Self);
    }

    void ResetAllSettingsBeforeSpawn()
    {
        spawnedXRotation = setRotation;
        spawnedYRotation = setRotation;
        spawnedZRotation = setRotation;
    }

    float MaxHeightFromCollider(GameObject g)
    {
        float height;
        CheckColliderState(g);
        if (Selection.activeGameObject != null)
        {
            Vector3 pos = g.transform.position;     // Start the raycast at the objects position
            Collider gCollider = g.GetComponent<Collider>();
            height = pos.y = gCollider.bounds.extents.y;
        }
        else
        {
            height = minHeight;
        }
        return height;
    }

    void ScaleAllBranches(float scaleAmount, bool children)
    {

        CheckColliderState(Selection.activeGameObject);
        Vector3 newScale;

        //if(children){
        foreach (GameObject g in Selection.gameObjects)
        {
            for (int i = 0; i < g.transform.childCount; i++)
            {
                newScale.x = scaleAmount; newScale.y = scaleAmount; newScale.z = scaleAmount;
                g.transform.GetChild(i).localScale = Vector3.Scale((new Vector3(newScale.x, newScale.y, newScale.z)), g.transform.GetChild(i).localScale);

            }

        }
        //}else{
        //newScale.x = scaleAmount; newScale.y = scaleAmount; newScale.z = scaleAmount;
        //Selection.activeGameObject.transform.localScale = Vector3.Scale( (new Vector3(newScale.x, newScale.y, newScale.z)),  Selection.activeGameObject.transform.localScale);
        //}
    }

    private void RotateChildren(float deg, bool children, rotAxis axis, bool space)
    {
        if (children)
        {
            foreach (GameObject g in Selection.gameObjects)
            {
                for (int i = 0; i < g.transform.childCount; i++)
                {

                    switch ((int)axis)
                    {
                        case 0:
                            if (space)
                            {
                                g.transform.GetChild(i).Rotate(deg, 0, 0, Space.Self);
                            }
                            else
                            {
                                g.transform.GetChild(i).Rotate(deg, 0, 0, Space.World);
                            }
                            break;
                        case 1:
                            if (space)
                            {
                                g.transform.GetChild(i).Rotate(0, deg, 0, Space.Self);
                            }
                            else
                            {
                                g.transform.GetChild(i).Rotate(0, deg, 0, Space.World);
                            }
                            break;
                        case 2:
                            if (space)
                            {
                                g.transform.GetChild(i).Rotate(0, 0, deg, Space.Self);
                            }
                            else
                            {
                                g.transform.GetChild(i).Rotate(0, 0, deg, Space.World);
                            }
                            break;

                    }
                }

            }
        }
        else
        {
            switch ((int)axis)
            {
                case 0:
                    if (space)
                    {
                        Selection.activeGameObject.transform.Rotate(deg, 0, 0, Space.Self);
                    }
                    else
                    {
                        Selection.activeGameObject.transform.Rotate(deg, 0, 0, Space.World);
                    }
                    break;
                case 1:
                    if (space)
                    {
                        Selection.activeGameObject.transform.Rotate(0, deg, 0, Space.Self);
                    }
                    else
                    {
                        Selection.activeGameObject.transform.Rotate(0, deg, 0, Space.World);
                    }
                    break;
                case 2:
                    if (space)
                    {
                        Selection.activeGameObject.transform.Rotate(0, 0, deg, Space.Self);
                    }
                    else
                    {
                        Selection.activeGameObject.transform.Rotate(0, 0, deg, Space.World);
                    }
                    break;

            }
            Selection.activeGameObject.transform.Rotate(0, deg, 0, Space.Self);
        }
    }

    void CheckColliderState(GameObject g)
    {
        if (Selection.activeGameObject != null)
        {
            //Collider gCollider = g.GetComponent<Collider> ();
            if (g.GetComponent<Collider>() == null)
            {
                Debug.Log("Mesh collliders have been added in order to generate branches");
                g.AddComponent<MeshCollider>().sharedMesh = g.GetComponent<MeshFilter>().sharedMesh;
            }
        }
    }

    private void GenerateBranchesX(GameObject g, GameObject spawned)
    {
        CheckColliderState(g);
        Vector3 pos = g.transform.position;     // Start the raycast at the objects position
        Quaternion rot = Quaternion.identity;
        Vector3 cast = Vector3.zero;
        Collider gCollider = g.GetComponent<Collider>();

        while (true)
        {
            pos += g.transform.right * (gCollider.bounds.extents.x != 0 ? gCollider.bounds.extents.x : 1) * 1.5f;
            rot = Quaternion.Euler(spawnedXRotation, 0, 0);
            cast = -g.transform.right;
            pos.y += Random.Range(minHeight, gCollider.bounds.extents.y);
            pos.z += Random.Range(-gCollider.bounds.extents.z, gCollider.bounds.extents.z);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch.transform.localScale);

        branch.name += "MobileFVbranch";
        branch.transform.parent = g.transform;
        RaycastFor(ref branch, ref g, cast);

        ////////////////////////////////////////////////////////////////////

        while (true)
        {
            pos -= g.transform.right * (gCollider.bounds.extents.x != 0 ? gCollider.bounds.extents.x : 1) * 1.5f;
            rot = Quaternion.Euler(spawnedXRotation, 0, 0);
            cast = g.transform.right;
            pos.y += Random.Range(minHeight, gCollider.bounds.extents.y);
            pos.z += Random.Range(-gCollider.bounds.extents.z, gCollider.bounds.extents.z);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch2 = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch2.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch2.transform.localScale);

        branch2.name += "MobileFVbranch";
        branch2.transform.parent = g.transform;
        RaycastFor(ref branch2, ref g, cast);
    }

    private void GenerateBranchesY(GameObject g, GameObject spawned)
    {
        CheckColliderState(g);
        Vector3 pos = g.transform.position;     // Start the raycast at the objects position
        Quaternion rot = Quaternion.identity;
        Vector3 cast = Vector3.zero;
        Collider gCollider = g.GetComponent<Collider>();

        while (true)
        {

            pos += g.transform.up * (gCollider.bounds.extents.y != 0 ? gCollider.bounds.extents.y : 1) * 1.5f;
            cast = -g.transform.up;
            rot = Quaternion.Euler(0, spawnedYRotation, 0);
            pos.x += Random.Range(-gCollider.bounds.extents.x, gCollider.bounds.extents.x);
            pos.z += Random.Range(-gCollider.bounds.extents.z, gCollider.bounds.extents.z);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch.transform.localScale);

        branch.name += "MobileFVbranch";
        branch.transform.parent = g.transform;
        RaycastFor(ref branch, ref g, cast);

        ////////////////////////////////////////////////////

        while (true)
        {

            pos -= g.transform.up * (gCollider.bounds.extents.y != 0 ? gCollider.bounds.extents.y : 1) * 1.5f;
            rot = Quaternion.Euler(0, spawnedYRotation, 0);
            cast = g.transform.up;
            pos.x += Random.Range(-gCollider.bounds.extents.x, gCollider.bounds.extents.x);
            pos.z += Random.Range(-gCollider.bounds.extents.z, gCollider.bounds.extents.z);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch2 = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch2.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch2.transform.localScale);

        branch2.name += "MobileFVbranch";
        branch2.transform.parent = g.transform;
        RaycastFor(ref branch2, ref g, cast);

    }

    private void GenerateBranchesZ(GameObject g, GameObject spawned)
    {
        CheckColliderState(g);

        Vector3 pos = g.transform.position;     // Start the raycast at the objects position
        Quaternion rot = Quaternion.identity;
        Vector3 cast = Vector3.zero;
        Collider gCollider = g.GetComponent<Collider>();

        while (true)
        {
            pos += g.transform.forward * (gCollider.bounds.extents.z != 0 ? gCollider.bounds.extents.z : 1) * 1.5f;
            rot = Quaternion.Euler(0, 0, spawnedZRotation);
            cast = -g.transform.forward;
            pos.x += Random.Range(-gCollider.bounds.extents.x, gCollider.bounds.extents.x);
            pos.y += Random.Range(minHeight, gCollider.bounds.extents.y);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch.transform.localScale);


        branch.name += "MobileFVbranch";
        branch.transform.parent = g.transform;
        RaycastFor(ref branch, ref g, cast);

        ////////////////////////////////////////////////

        while (true)
        {
            pos -= g.transform.forward * (gCollider.bounds.extents.z != 0 ? gCollider.bounds.extents.z : 1) * 1.5f;
            rot = Quaternion.Euler(0, 0, spawnedZRotation);
            cast = g.transform.forward;
            pos.x += Random.Range(-gCollider.bounds.extents.x, gCollider.bounds.extents.x);
            pos.y += Random.Range(minHeight, gCollider.bounds.extents.y);

            if (RaycastFor(pos, g, cast))
                break;
            else
            {
                pos = g.transform.position;

            }
        }

        GameObject branch2 = GameObject.Instantiate(spawned, pos, rot) as GameObject;
        scaleUp.x = scalingValue; scaleUp.y = scalingValue; scaleUp.z = scalingValue;
        branch2.transform.localScale = Vector3.Scale((new Vector3(scaleUp.x, scaleUp.y, scaleUp.z)), branch2.transform.localScale);


        branch2.name += "MobileFVbranch";
        branch2.transform.parent = g.transform;
        RaycastFor(ref branch2, ref g, cast);

    }


    private bool RaycastFor(Vector3 sender, GameObject end, Vector3 direction)
    {
        RaycastHit hit;

        float prevDist = 0;
        while (true)
        {
            prevDist = Vector3.Distance(sender, end.transform.position);
            if (Physics.Raycast(new Ray(sender, direction), out hit))
            {
                if (hit.transform == end.transform)
                    return true;
                else
                {
                    sender = hit.point + (direction * 5f);
                    if (Vector3.Distance(sender, end.transform.position) > prevDist)
                        return false;
                }
            }
            else
                return false;
        }
    }

    private void RaycastFor(ref GameObject sender, ref GameObject end, Vector3 direction)
    {
        RaycastHit hit;

        Vector3 start = sender.transform.position;
        float prevDist = 0;
        while (true)
        {
            prevDist = Vector3.Distance(start, end.transform.position);
            if (Physics.Raycast(new Ray(start, direction), out hit))
            {
                if (hit.transform == end.transform)
                {
                    sender.transform.position = hit.point;
                    sender.transform.up = hit.normal;

                    break;
                }
                else
                {
                    start = hit.point + (direction * 5f);
                    if (Vector3.Distance(start, end.transform.position) > prevDist)
                        break;
                }
            }
            else
                break;
        }
    }





    void MakeBranches(GameObject obj)
    {
        CheckColliderState(obj);

        if (deleteOld)
            DeleteOldBranches(obj);


        // Start generating each object
        for (int i = 0; i < count; i++)
        {
            if (!ofEach)
            {
                GenerateBranchesX(obj, spawners[Random.Range(0, spawners.Count)]);

                GenerateBranchesY(obj, spawners[Random.Range(0, spawners.Count)]);

                GenerateBranchesZ(obj, spawners[Random.Range(0, spawners.Count)]);

            }
            else
            {
                foreach (GameObject s in spawners)
                {
                    GenerateBranchesX(obj, s);

                    GenerateBranchesY(obj, s);

                    GenerateBranchesZ(obj, s);

                }
            }

        }

    }






}
#endif
