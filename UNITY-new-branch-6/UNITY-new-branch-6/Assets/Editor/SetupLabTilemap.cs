#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

[InitializeOnLoad]
public static class SetupLabTilemap
{
    static SetupLabTilemap()
    {
        // Automatically execute on compile to ensure it runs without requiring user interaction
        if (!SessionState.GetBool("LabTilemapSetupDone", false))
        {
            SessionState.SetBool("LabTilemapSetupDone", true);
            EditorApplication.delayCall += RunSetup;
        }
    }

    [MenuItem("Tools/Setup Lab Tilemap")]
    public static void RunSetup()
    {
        Debug.Log("Starting Lab Tilemap setup...");

        // Find all PNG files in Assets starting with or containing LabBPlatform
        string[] files = Directory.GetFiles("Assets", "*LabBPlatform*.png", SearchOption.AllDirectories);

        int tilesCreated = 0;

        foreach (string file in files)
        {
            string assetPath = file.Replace('\\', '/');
            string tilesFolder = Path.GetDirectoryName(assetPath).Replace('\\', '/');

            // Load all assets at path (including sliced sprites)
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object subAsset in allAssets)
            {
                if (subAsset is Sprite sprite)
                {
                    string tileName = sprite.name + "_Tile.asset";
                    string tilePath = $"{tilesFolder}/{tileName}";

                    // Create Tile asset
                    Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = sprite;
                        tile.name = sprite.name;
                        AssetDatabase.CreateAsset(tile, tilePath);
                        tilesCreated++;
                    }
                    else
                    {
                        tile.sprite = sprite;
                        EditorUtility.SetDirty(tile);
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Created/Updated {tilesCreated} Tile assets.");

        // 3. Open Laboratory scene and build Grid/Tilemap
        string scenePath = "Assets/Scenes/Laboratory.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        if (!scene.IsValid())
        {
            Debug.LogError($"Could not open scene at {scenePath}");
            return;
        }

        // Find or create Grid
        Grid grid = Object.FindAnyObjectByType<Grid>();
        GameObject gridGo;
        if (grid == null)
        {
            gridGo = new GameObject("Grid");
            grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);
            Undo.RegisterCreatedObjectUndo(gridGo, "Create Grid");
        }
        else
        {
            gridGo = grid.gameObject;
        }

        // Find or create Tilemap under Grid
        Tilemap tilemap = Object.FindAnyObjectByType<Tilemap>();
        GameObject tilemapGo;
        if (tilemap == null)
        {
            tilemapGo = new GameObject("Tilemap_Platforms");
            tilemapGo.transform.SetParent(gridGo.transform);
            tilemap = tilemapGo.AddComponent<Tilemap>();
            
            var tilemapRenderer = tilemapGo.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortOrder = TilemapRenderer.SortOrder.TopLeft;
            
            var collider = tilemapGo.AddComponent<TilemapCollider2D>();
            
            Undo.RegisterCreatedObjectUndo(tilemapGo, "Create Tilemap");
            Debug.Log("Created Grid and Tilemap_Platforms in Laboratory scene.");
        }
        else
        {
            tilemapGo = tilemap.gameObject;
            if (tilemapGo.GetComponent<TilemapCollider2D>() == null)
            {
                tilemapGo.AddComponent<TilemapCollider2D>();
            }
            Debug.Log("Found existing Tilemap in scene.");
        }

        // Save scene changes
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Laboratory scene saved successfully with Tilemap setup!");
    }
}
#endif
