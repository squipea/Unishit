using UnityEngine;

/// <summary>
/// Spawns invisible left/right walls at scene edges (from CameraBounds or manual X).
/// Skips creation if LeftWall_Boundary already exists (e.g. Laboratory).
/// </summary>
[DefaultExecutionOrder(-100)]
public class SceneBoundaryWalls : MonoBehaviour
{
    [Header("Bounds source")]
    public bool useCameraBounds = true;
    public float horizontalInset = 0.5f;

    [Header("Manual bounds (used when CameraBounds is missing)")]
    public float leftWallX = -100f;
    public float rightWallX = 100f;

    [Header("Wall dimensions")]
    public float wallCenterY = 2f;
    public float wallHeight = 14f;
    public float wallThickness = 0.3f;

    [Header("Layers")]
    [Tooltip("Default (0) so movement wall-checks match LeftWall_Boundary objects.")]
    public int wallLayer = 0;

    void Awake()
    {
        PhysicsMaterial2D wallMaterial = Resources.Load<PhysicsMaterial2D>("NoFriction");
        ApplyMaterialToExistingWalls(wallMaterial);

        if (FindBoundaryWall("LeftWall_Boundary") != null)
            return;

        if (useCameraBounds && TryGetCameraBounds(out Bounds bounds))
        {
            leftWallX = bounds.min.x + horizontalInset;
            rightWallX = bounds.max.x - horizontalInset;
            wallCenterY = bounds.center.y;
            wallHeight = Mathf.Max(bounds.size.y + 8f, wallHeight);
        }

        CreateWall("LeftWall_Boundary", leftWallX, wallMaterial);
        CreateWall("RightWall_Boundary", rightWallX, wallMaterial);
    }

    static void ApplyMaterialToExistingWalls(PhysicsMaterial2D material)
    {
        if (material == null)
            return;

        ApplyMaterialToWall(FindBoundaryWall("LeftWall_Boundary"), material);
        ApplyMaterialToWall(FindBoundaryWall("RightWall_Boundary"), material);
    }

    static void ApplyMaterialToWall(GameObject wall, PhysicsMaterial2D material)
    {
        if (wall == null)
            return;

        var collider = wall.GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.sharedMaterial = material;
    }

    static GameObject FindBoundaryWall(string wallName)
    {
        var wall = GameObject.Find(wallName);
        return wall;
    }

    static bool TryGetCameraBounds(out Bounds bounds)
    {
        bounds = default;
        var cameraBounds = GameObject.Find("CameraBounds");
        if (cameraBounds == null)
            return false;

        var polygon = cameraBounds.GetComponent<PolygonCollider2D>();
        if (polygon != null)
        {
            bounds = polygon.bounds;
            return true;
        }

        var box = cameraBounds.GetComponent<BoxCollider2D>();
        if (box != null)
        {
            bounds = box.bounds;
            return true;
        }

        return false;
    }

    void CreateWall(string wallName, float x, PhysicsMaterial2D material)
    {
        var wall = new GameObject(wallName);
        wall.layer = wallLayer;
        wall.transform.SetParent(transform, false);
        wall.transform.position = new Vector3(x, wallCenterY, 0f);

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(wallThickness, wallHeight);
        if (material != null)
            collider.sharedMaterial = material;

        var rb = wall.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }
}
