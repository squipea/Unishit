#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;

[InitializeOnLoad]
public static class SetupBossBackground
{
    static SetupBossBackground()
    {
        // Automatically run once on compilation to setup the scene
        if (!SessionState.GetBool("BossBackgroundSetupDone", false))
        {
            SessionState.SetBool("BossBackgroundSetupDone", true);
            EditorApplication.delayCall += RunSetup;
        }
    }

    [MenuItem("Tools/Setup Boss Background")]
    public static void RunSetup()
    {
        Debug.Log("Starting Boss Background setup...");

        // 1. Open Laboratory scene
        string scenePath = "Assets/Scenes/Laboratory.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene at {scenePath}");
            return;
        }

        // 2. Find or create BossBackground GameObject
        GameObject bgGo = GameObject.Find("BossBackground");
        if (bgGo == null)
        {
            bgGo = new GameObject("BossBackground");
            Undo.RegisterCreatedObjectUndo(bgGo, "Create Boss Background");
        }

        // 3. Configure SpriteRenderer (Set rendering layer below tentacles)
        SpriteRenderer sr = bgGo.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = bgGo.AddComponent<SpriteRenderer>();
        }
        sr.sortingOrder = -5; // Behind player, enemies, and tentacles

        // Center the background in the boss arena
        // The boss arena tentacles spawn between X: 26 and X: 58. The center is around X: 42, Y: 0
        bgGo.transform.position = new Vector3(42f, 0f, 0f);
        bgGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f); // Set scale appropriately to fit the screen size

        // 4. Configure BossBackgroundController
        BossBackgroundController controller = bgGo.GetComponent<BossBackgroundController>();
        if (controller == null)
        {
            controller = bgGo.AddComponent<BossBackgroundController>();
        }

        // 5. Load and Sort Sprites
        string bgFolder = "Assets/Sprites (Enemy)/Boss BG";
        string dmgFolder = "Assets/Sprites (Enemy)/Boss Damage";

        if (Directory.Exists(bgFolder))
        {
            // Load and sort all boss bg sprites
            Sprite[] allBgSprites = AssetDatabase.FindAssets("t:Sprite", new[] { bgFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(s => s != null)
                .OrderBy(s => ExtractNumber(s.name))
                .ToArray();

            // Distribute sprites as requested:
            // idleSprites: Boss bg 1 - 17
            controller.idleSprites = allBgSprites.Where(s => ExtractNumber(s.name) >= 1 && ExtractNumber(s.name) <= 17).ToArray();
            
            // Assign the first frame by default so it is visible in the Editor / Scene View
            if (controller.idleSprites.Length > 0 && sr.sprite == null)
            {
                sr.sprite = controller.idleSprites[0];
            }
            
            // deathSprites: Boss bg 18 - 20
            controller.deathSprites = allBgSprites.Where(s => ExtractNumber(s.name) >= 18 && ExtractNumber(s.name) <= 20).ToArray();
            
            // regenSprites: Boss bg 20 - 22
            controller.regenSprites = allBgSprites.Where(s => ExtractNumber(s.name) >= 20 && ExtractNumber(s.name) <= 22).ToArray();

            Debug.Log($"Loaded {controller.idleSprites.Length} idle, {controller.deathSprites.Length} death, and {controller.regenSprites.Length} regen sprites.");
        }
        else
        {
            Debug.LogError($"Boss BG folder not found at: {bgFolder}");
        }

        if (Directory.Exists(dmgFolder))
        {
            // Load and sort all damage sprites
            controller.damageSprites = AssetDatabase.FindAssets("t:Sprite", new[] { dmgFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(s => s != null)
                .OrderBy(s => ExtractNumber(s.name))
                .ToArray();

            Debug.Log($"Loaded {controller.damageSprites.Length} damage sprites.");
        }
        else
        {
            Debug.LogError($"Boss Damage folder not found at: {dmgFolder}");
        }

        // 6. Load Audio Clips
        AudioClip idleClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BOSS IDLE.mp3");
        AudioClip damageClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BOSS DAMAGED.mp3");

        if (idleClip != null)
            controller.bossIdleClip = idleClip;
        else
            Debug.LogWarning("Could not find BOSS IDLE.mp3 at Assets/Audio/BOSS IDLE.mp3");

        if (damageClip != null)
            controller.bossDamageAndDiedClip = damageClip;
        else
            Debug.LogWarning("Could not find BOSS DAMAGED.mp3 at Assets/Audio/BOSS DAMAGED.mp3");

        Debug.Log($"Audio clips assigned: Idle={idleClip != null}, BossDamaged={damageClip != null}");

        // Save changes
        EditorUtility.SetDirty(bgGo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Boss Background successfully configured and scene saved!");
    }

    private static int ExtractNumber(string name)
    {
        string numberStr = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        return int.TryParse(numberStr, out int val) ? val : 0;
    }
}
#endif
