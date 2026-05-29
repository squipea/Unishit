#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupBossLevelStage
{
    [MenuItem("Tools/Setup Boss Level Stage")]
    public static void RunSetup()
    {
        Debug.Log("Starting Boss Level Stage setup...");

        // 1. Open Laboratory scene
        string scenePath = "Assets/Scenes/Laboratory.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene at {scenePath}");
            return;
        }

        // 2. Find Player GameObject
        GameObject playerGo = GameObject.Find("Player");
        if (playerGo == null)
        {
            playerGo = GameObject.FindWithTag("Player");
        }

        if (playerGo != null)
        {
            Undo.RecordObject(playerGo.transform, "Position Player on Initial Platform");
            playerGo.transform.position = new Vector3(-150f, -3.2f, 0f);
            Debug.Log($"Repositioned Player to starting platform: {playerGo.transform.position}");
        }
        else
        {
            Debug.LogError("Could not find Player GameObject in the scene!");
        }

        // 3. Create or find InitialStageParent holding all starting objects
        GameObject initialStageParent = GameObject.Find("InitialStageParent");
        if (initialStageParent == null)
        {
            initialStageParent = new GameObject("InitialStageParent");
            Undo.RegisterCreatedObjectUndo(initialStageParent, "Create Initial Stage Parent");
        }
        initialStageParent.transform.position = new Vector3(-150f, 0f, 0f);

        // 4. Create or find Initial Platform
        GameObject platformGo = GameObject.Find("InitialPlatform");
        if (platformGo == null)
        {
            platformGo = new GameObject("InitialPlatform");
            platformGo.transform.SetParent(initialStageParent.transform);
            platformGo.transform.localPosition = new Vector3(0f, -4.5f, 0f);
            
            // Add BoxCollider2D
            BoxCollider2D collider = platformGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(25f, 1f);

            // Add SpriteRenderer so it is visible in the scene
            SpriteRenderer sr = platformGo.AddComponent<SpriteRenderer>();
            // Load a default square sprite from Unity built-ins
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Sleek dark lab aesthetic color
            sr.size = new Vector2(25f, 1f);
            sr.drawMode = SpriteDrawMode.Sliced;

            Undo.RegisterCreatedObjectUndo(platformGo, "Create Initial Platform");
            Debug.Log("Created new initial platform at X: -150");
        }

        // 5. Create Boundaries (Left/Right Walls) so player cannot walk off
        GameObject leftWall = GameObject.Find("InitialLeftWall");
        if (leftWall == null)
        {
            leftWall = new GameObject("InitialLeftWall");
            leftWall.transform.SetParent(initialStageParent.transform);
            leftWall.transform.localPosition = new Vector3(-12.5f, 0f, 0f);
            BoxCollider2D col = leftWall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 10f);
            Undo.RegisterCreatedObjectUndo(leftWall, "Create Initial Left Wall");
        }

        GameObject rightWall = GameObject.Find("InitialRightWall");
        if (rightWall == null)
        {
            rightWall = new GameObject("InitialRightWall");
            rightWall.transform.SetParent(initialStageParent.transform);
            rightWall.transform.localPosition = new Vector3(12.5f, 0f, 0f);
            BoxCollider2D col = rightWall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 10f);
            Undo.RegisterCreatedObjectUndo(rightWall, "Create Initial Right Wall");
        }

        // 6. Spawn Soldier Enemy on this platform
        GameObject soldierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Soldier.prefab");
        if (soldierPrefab != null)
        {
            // Remove existing InitialSoldier if any to prevent duplicates on multiple runs
            GameObject existingSoldier = GameObject.Find("InitialSoldier");
            if (existingSoldier != null)
            {
                Undo.DestroyObjectImmediate(existingSoldier);
            }

            GameObject soldierGo = (GameObject)PrefabUtility.InstantiatePrefab(soldierPrefab);
            soldierGo.name = "InitialSoldier";
            soldierGo.transform.position = new Vector3(-144f, -3.2f, 0f);

            // Add the stage transition script to this soldier
            InitialSoldierStageTransition transition = soldierGo.GetComponent<InitialSoldierStageTransition>();
            if (transition == null)
            {
                transition = soldierGo.AddComponent<InitialSoldierStageTransition>();
            }
            transition.bossSpawnPosition = new Vector3(-41.71f, -1.2f, 0f);

            Undo.RegisterCreatedObjectUndo(soldierGo, "Spawn Initial Soldier");
            Debug.Log($"Successfully spawned Soldier enemy and attached InitialSoldierStageTransition script at: {soldierGo.transform.position}");
        }
        else
        {
            Debug.LogError("Could not find Soldier prefab at Assets/Prefabs/Enemies/Soldier.prefab");
        }

        // 7. Save scene changes
        EditorUtility.SetDirty(initialStageParent);
        if (playerGo != null) EditorUtility.SetDirty(playerGo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Laboratory scene successfully configured with Initial Platform and Soldier enemy!");
    }
}
#endif
