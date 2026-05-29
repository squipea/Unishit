using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;

    [Header("Offset")]
    public float offsetX = 0f;
    public float offsetY = 1.2f;

    [Header("Vertical Clamp")]
    [Tooltip("Optional. Camera Y will never go below the top of this object.")]
    public Transform groundReference;
    [Tooltip("If groundReference is not assigned, the script will search for a GameObject with this name.")]
    public string groundReferenceName = "LongWhiteGround";

    private float minY = float.NegativeInfinity;
    private bool minYResolved;

    void Start()
    {
        ResolveMinY();
    }

    private void ResolveMinY()
    {
        if (groundReference == null && !string.IsNullOrEmpty(groundReferenceName))
        {
            GameObject go = GameObject.Find(groundReferenceName);
            if (go != null)
                groundReference = go.transform;
        }

        if (groundReference != null)
        {
            // Use the collider bounds top if available, otherwise use transform Y
            Collider2D col = groundReference.GetComponent<Collider2D>();
            if (col != null)
            {
                minY = col.bounds.max.y;
            }
            else
            {
                Renderer rend = groundReference.GetComponent<Renderer>();
                if (rend != null)
                    minY = rend.bounds.max.y;
                else
                    minY = groundReference.position.y;
            }
        }

        minYResolved = true;
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (!minYResolved)
            ResolveMinY();

        float targetY = player.position.y + offsetY;

        if (minY > float.NegativeInfinity)
            targetY = Mathf.Max(targetY, minY);

        transform.position = new Vector3(
            player.position.x + offsetX,
            targetY,
            transform.position.z
        );
    }
}