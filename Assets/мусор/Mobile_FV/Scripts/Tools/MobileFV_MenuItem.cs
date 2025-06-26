using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
//============================================================================
//
//			Used for Mobile ForestVision Menu Items
//
//============================================================================

public static class MobileFV_MenuItems
{

    [MenuItem("Tools/MobileFV Tools", false, 30)]
    private static void ShowTools()
    {
        MobileFV_Tools.ShowEditor();
    }

    #region Seasonal Trees
    [MenuItem("GameObject/3D Object/Mobile FV/Trees/5 Arms", false, 30)]
    private static void CreateSeasonalTree5Arms()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_5Arms", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Conifer Short", false, 30)]
    private static void CreateSeasonalTreeConiferShort()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_ConiferShort", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Conifer Tall", false, 30)]
    private static void CreateSeasonalTreeConiferTall()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_ConiferTall", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Conifer Young", false, 30)]
    private static void CreateSeasonalTreeConiferYoung()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_ConiferYoung", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Full Tree", false, 30)]
    private static void CreateSeasonalTreeFull()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_FullTree", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Shorty", false, 30)]
    private static void CreateSeasonalTreeShorty()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_short", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Tall Bunches", false, 30)]
    private static void CreateSeasonalTreeTallBunches()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_single", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Skinny Bunches", false, 30)]
    private static void CreateSeasonalTreeSkinnyBunches()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_SkinnyBunches", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Skinny Tall", false, 30)]
    private static void CreateSeasonalTreeSkinnyTall()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_SkinnyTall", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Tall Thin", false, 30)]
    private static void CreateSeasonalTreeTallThin()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_TallThin", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Young", false, 30)]
    private static void CreateSeasonalTreeYoung()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Tree_young", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trees/Curved", false, 30)]
    private static void CreateSeasonalTreeCurved()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__TreeCurved", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    #endregion

    #region Seasonal Trunks
    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Basic", false, 30)]
    private static void CreateSeasonalTrunkBasic()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Branching", false, 30)]
    private static void CreateSeasonalTrunkBranching()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_branching_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Curvy", false, 30)]
    private static void CreateSeasonalTrunkCurvy()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_curvy_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Jungle 1", false, 30)]
    private static void CreateSeasonalTrunkJungle1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_jungle1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Jungle 2", false, 30)]
    private static void CreateSeasonalTrunkJungle2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_jungle2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Short Curved", false, 30)]
    private static void CreateSeasonalTrunkShortCurved()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_shortCurved_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Simple 2 Branch", false, 30)]
    private static void CreateSeasonalTrunkSimple2B()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_simple2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Simple 4 Branch", false, 30)]
    private static void CreateSeasonalTrunkSimple4B()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_simple4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Tall 1", false, 30)]
    private static void CreateSeasonalTrunkTall1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_tall1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Trunks/Tall 2", false, 30)]
    private static void CreateSeasonalTrunkTall2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__trunk_tall2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    #endregion

    #region Seasonal Stumps
    [MenuItem("GameObject/3D Object/Mobile FV/Stumps/Tall", false, 30)]
    private static void CreateSeasonalStumpTall()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__stump1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Stumps/Short", false, 30)]
    private static void CreateSeasonalStumpShort()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__stump2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Stumps/Low", false, 30)]
    private static void CreateSeasonalStumpLow()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__stump3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Stumps/Nub", false, 30)]
    private static void CreateSeasonalStumpNub()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__stump4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Stumps/Broken", false, 30)]
    private static void CreateSeasonalStumpBroken()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__stump5_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    #endregion

    #region Seasonal Branches
    [MenuItem("GameObject/3D Object/Mobile FV/Branches/v0", false, 30)]
    private static void CreateSeasonalBranchEmptySimple()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__branch0_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/v1", false, 30)]
    private static void CreateSeasonalBranchEmptySimpleBent()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__branch1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/v2", false, 30)]
    private static void CreateSeasonalBranchEmptyBare1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__branch2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/v3", false, 30)]
    private static void CreateSeasonalBranchEmptyBare2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__branch3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/v4", false, 30)]
    private static void CreateSeasonalBranchEmptyBare3()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__branch4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/Low Poly v1", false, 30)]
    private static void CreateSeasonalBranchLPBare1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__deadBranch1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/Low Poly v2", false, 30)]
    private static void CreateSeasonalBranchLPBare2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__deadBranch2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/Low Poly v3", false, 30)]
    private static void CreateSeasonalBranchLPBare3()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__deadBranch3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Branches/Low Poly v4", false, 30)]
    private static void CreateSeasonalBranchLPBare4()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__deadBranch4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }



    #endregion

    #region Seasonal Cards

    [MenuItem("GameObject/3D Object/Mobile FV/Cards/Leaf Filler", false, 30)]
    private static void CreateSeasonalCardLeafFiller()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__card1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Cards/Bare Branch 1", false, 30)]
    private static void CreateSeasonalCardBB1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__emptyBranch1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }


    [MenuItem("GameObject/3D Object/Mobile FV/Cards/Leaves 1", false, 30)]
    private static void CreateSeasonalCardLeaves1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__leaves1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }


    [MenuItem("GameObject/3D Object/Mobile FV/Cards/Leaves 2", false, 30)]
    private static void CreateSeasonalCardLeaves2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__leaves2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    #endregion

    #region Seasonal Foliage
    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/LeafBunch", false, 30)]
    private static void CreateSeasonalFoliageBunch()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__leafBunch_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/v1", false, 30)]
    private static void CreateSeasonalFoliage1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__foliage1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/v2", false, 30)]
    private static void CreateSeasonalFoliage2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__foliage2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/v3", false, 30)]
    private static void CreateSeasonalFoliage3()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__foliage3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/v4", false, 30)]
    private static void CreateSeasonalFoliage4()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__foliage4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/v5", false, 30)]
    private static void CreateSeasonalFoliage5()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__foliage5_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/Conifer Short", false, 30)]
    private static void CreateSeasonalConiferShort()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Conifer_short", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Foliage/Conifer Long", false, 30)]
    private static void CreateSeasonalConiferLong()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__Conifer_long", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    #endregion

    #region Seasonal Ground Cover
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Grass/v1", false, 30)]
    private static void CreateSeasonalGrass()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__grass1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Grass/v2", false, 30)]
    private static void CreateSeasonalGrass2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__grass2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Grass/v3", false, 30)]
    private static void CreateSeasonalGrass3()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__grass3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Grass/v4", false, 30)]
    private static void CreateSeasonalGrass4()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__grass4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Grass/v5", false, 30)]
    private static void CreateSeasonalGrass5()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__grass5_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }


    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v1", false, 30)]
    private static void CreateSeasonalWeeds1()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed1_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v2", false, 30)]
    private static void CreateSeasonalWeeds2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed2_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v3", false, 30)]
    private static void CreateSeasonalWeeds3()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed3_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v4", false, 30)]
    private static void CreateSeasonalWeeds4()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed4_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v5", false, 30)]
    private static void CreateSeasonalWeeds5()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed5_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v6", false, 30)]
    private static void CreateSeasonalWeeds6()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed6_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v7", false, 30)]
    private static void CreateSeasonalWeeds7()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed7_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v8", false, 30)]
    private static void CreateSeasonalWeeds8()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed8_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }
    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Weeds/v9", false, 30)]
    private static void CreateSeasonalWeeds9()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__weed9_v1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }


    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Fern/Plant", false, 30)]
    private static void CreateSeasonalFern()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__fernPlant1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    [MenuItem("GameObject/3D Object/Mobile FV/Ground Cover/Fern/Leaf", false, 30)]
    private static void CreateSeasonalFern2()
    {
        Selection.activeGameObject = PrefabUtility.InstantiatePrefab(Resources.Load("C1__fern1", typeof(GameObject)), (Selection.activeGameObject != null) ? Selection.activeGameObject.transform : null) as GameObject;
    }

    #endregion


}

#endif