using UnityEngine;
using Unity.Cinemachine;

public class BossCameraController : MonoBehaviour
{
    public static BossCameraController Instance { get; private set; }

    public CinemachineCamera playerFollowCamera;
    public CinemachineCamera bossCamera;
    public float bossOrthoSize = 16f;
    public GameObject arenaWalls;
    public bool bossStageUnlockedOnStart = false;

    private bool bossStageUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (bossCamera != null)
            {
                bossCamera.Lens.OrthographicSize = bossOrthoSize;
                // Make boss camera static (not following player)
                bossCamera.Follow = null; 
                bossCamera.LookAt = null;
                bossCamera.Priority = 5;
            }

            bossStageUnlocked = bossStageUnlockedOnStart;
            if (arenaWalls != null)
                arenaWalls.SetActive(bossStageUnlockedOnStart);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnterBossMode()
    {
        if (!bossStageUnlocked)
            return;

        if (bossCamera != null)
        {
            bossCamera.Lens.OrthographicSize = bossOrthoSize;
            bossCamera.Priority = 20;

            // Disable confiner so the static camera isn't squished/snapped by the 2D bounding shape boundaries
            var confiner = bossCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
            {
                confiner.enabled = false;
            }

            if (arenaWalls != null)
            {
                arenaWalls.SetActive(true);
                // Fix: The BossLeftWall is positioned at X=15 by default, which blocks the player from reaching the boss.
                // Move it to X=2 (just behind the trigger zone) so it traps the player inside instead.
                Transform leftWall = arenaWalls.transform.Find("BossLeftWall");
                if (leftWall != null)
                {
                    leftWall.localPosition = new Vector3(2f, leftWall.localPosition.y, leftWall.localPosition.z);
                }
            }
        }

        // Start boss idle music the moment the boss camera kicks in
        if (BossBackgroundController.Instance != null)
            BossBackgroundController.Instance.StartBossMusic();
    }

    public void UnlockBossStage()
    {
        bossStageUnlocked = true;
    }

    public void UnlockBossStageAndEnter()
    {
        UnlockBossStage();
        EnterBossMode();
    }

    public void ExitBossMode()
    {
        if (bossCamera != null)
        {
            bossCamera.Priority = 5;
            if (arenaWalls != null) arenaWalls.SetActive(false);
        }
    }
}