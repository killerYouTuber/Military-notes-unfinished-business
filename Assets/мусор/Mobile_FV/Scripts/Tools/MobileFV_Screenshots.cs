using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]
public class MobileFV_Screenshots : EditorWindow
{

    int resWidth = 7680;
    int resHeight = 4320;
    private Camera myCamera;
    int scale = 1;
    string path = "";
    RenderTexture renderTexture;
    bool isTransparent = false;

    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MobileFV_Screenshots));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Screenshot");

    }

    void OnGUI()
    {
        GUILayout.Label("Where do you want the rendered image?", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(path, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
            path = EditorUtility.SaveFolderPanel("Path to Save Images", path, Application.dataPath);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Set Specific Resolution to Render", EditorStyles.boldLabel);
        resWidth = EditorGUILayout.IntField("Pixel Width", resWidth);
        resHeight = EditorGUILayout.IntField("Pixel Height", resHeight);

        myCamera = Camera.main;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Default 8K Size", GUILayout.MinHeight(40), GUILayout.MaxWidth(155)))
        {
            resWidth = 7680;
            resHeight = 4320;
            scale = 1;
        }
        if (GUILayout.Button("4K", GUILayout.MinHeight(40), GUILayout.MaxWidth(155)))
        {
            resWidth = 3840;
            resHeight = 2160;
            scale = 1;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("2K", GUILayout.MinHeight(40), GUILayout.MaxWidth(155)))
        {
            resWidth = 2040;
            resHeight = 1080;
            scale = 1;
        }
        if (GUILayout.Button("1K", GUILayout.MinHeight(40), GUILayout.MaxWidth(155)))
        {
            resWidth = 1024;
            resHeight = 512;
            scale = 1;
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Set To Game Window", GUILayout.MinHeight(40)))
        {
            resWidth = (int)Handles.GetMainGameViewSize().y;
            resHeight = (int)Handles.GetMainGameViewSize().x;

        }



        scale = EditorGUILayout.IntSlider("Scale", scale, 1, 8);



        EditorGUILayout.Space();
        //EditorGUILayout.LabelField ("Screenshot render will be: " + resWidth*scale + " x " + resHeight*scale + " px", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Render: \n" + resWidth * scale + " x " + resHeight * scale + " px", GUILayout.MinHeight(70)))
        {
            if (path == "")
            {
                path = EditorUtility.SaveFolderPanel("Path to Save Images", path, Application.dataPath);
                Debug.Log("Path Set");
                TakeHiResShot();
            }
            else
            {
                TakeHiResShot();
            }
        }

        if (GUILayout.Button("Open Screenshot \nLocation", GUILayout.MinHeight(70), GUILayout.MaxWidth(130)))
        {

            Application.OpenURL("file://" + path);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();



        if (takeHiResShot)
        {
            int resWidthN = resWidth * scale;
            int resHeightN = resHeight * scale;
            RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
            myCamera.targetTexture = rt;

            TextureFormat tFormat;
            if (isTransparent)
                tFormat = TextureFormat.ARGB32;
            else
                tFormat = TextureFormat.RGB24;


            Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
            myCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
            myCamera.targetTexture = null;
            RenderTexture.active = null;
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidthN, resHeightN);

            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            Application.OpenURL(filename);
            takeHiResShot = false;
        }



    }



    private bool takeHiResShot = false;
    public string lastScreenshot = "";


    public string ScreenShotName(int width, int height)
    {

        string strPath = "";

        strPath = string.Format("{0}/screen_{1}x{2}_{3}.png", path, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        lastScreenshot = strPath;

        return strPath;
    }



    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

}



#endif