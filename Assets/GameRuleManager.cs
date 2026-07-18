using System.Collections;
using TMPro;
using UnityEngine;

public class GameRuleManager : MonoBehaviour
{
    [Header("Start Points")]
    public Transform[] startPoints;

    [Header("Point UI")]
    public TextMeshProUGUI pointText;
    public GameObject winText;

    [Header("Rule")]
    public int totalPointCount = 4;

    [Header("Find Player")]
    public float findPlayerInterval = 0.2f;

    private PlayerMovement playerMovement;
    private int currentPointCount = 0;
    private bool startPositionSet = false;
    private bool gameFinished = false;

    void Start()
    {
        UpdatePointText();

        if (winText != null)
        {
            winText.SetActive(false);
        }

        StartCoroutine(FindPlayerAndSetStart());
    }

    IEnumerator FindPlayerAndSetStart()
    {
        while (!startPositionSet)
        {
            FindPlayerMovement();

            if (playerMovement != null)
            {
                SetStartPositionByPlayerId();
                startPositionSet = true;
                yield break;
            }

            yield return new WaitForSeconds(findPlayerInterval);
        }
    }

    void FindPlayerMovement()
    {
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (PlayerMovement player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (!player.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (player.Object != null && player.Object.HasStateAuthority)
            {
                playerMovement = player;
                Debug.Log("操作対象のPlayerMovementを見つけました: " + player.name);
                return;
            }
        }

        foreach (PlayerMovement player in players)
        {
            if (player == null)
            {
                continue;
            }

            if (!player.gameObject.activeInHierarchy)
            {
                continue;
            }

            playerMovement = player;
            Debug.Log("PlayerMovementを仮で設定しました: " + player.name);
            return;
        }
    }

    void SetStartPositionByPlayerId()
    {
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovementが見つかりません");
            return;
        }

        if (startPoints == null || startPoints.Length == 0)
        {
            Debug.LogWarning("StartPointsが設定されていません");
            return;
        }

        int index = GetStartPointIndex(playerMovement);
        Transform selectedStartPoint = startPoints[index];

        CharacterController controller =
            playerMovement.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        Vector3 safePosition =
            selectedStartPoint.position + Vector3.up * 0.5f;

        playerMovement.transform.position = safePosition;
        playerMovement.transform.rotation = selectedStartPoint.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

        PlayerRespawnPoint respawnPoint =
            playerMovement.GetComponent<PlayerRespawnPoint>();

        if (respawnPoint == null)
        {
            respawnPoint =
                playerMovement.gameObject.AddComponent<PlayerRespawnPoint>();
        }

        respawnPoint.SetRespawnPoint(
            safePosition,
            selectedStartPoint.rotation
        );

        Debug.Log("PlayerをStartPoint_" + (index + 1) + " に移動しました");
    }

    int GetStartPointIndex(PlayerMovement player)
    {
        if (player.Object != null)
        {
            int playerId = player.Object.InputAuthority.PlayerId;

            if (playerId <= 0)
            {
                playerId = player.Object.StateAuthority.PlayerId;
            }

            if (playerId > 0)
            {
                return (playerId - 1) % startPoints.Length;
            }
        }

        return 0;
    }

    public void GetPoint(PointItem pointItem)
    {
        if (gameFinished)
        {
            return;
        }

        if (pointItem == null)
        {
            return;
        }

        if (pointItem.IsCollected())
        {
            return;
        }

        pointItem.Collect();

        currentPointCount++;
        UpdatePointText();

        Debug.Log("Point: " + currentPointCount + " / " + totalPointCount);

        if (currentPointCount >= totalPointCount)
        {
            FinishGame();
        }
    }

    void UpdatePointText()
    {
        if (pointText != null)
        {
            pointText.text = "Point : " + currentPointCount + " / " + totalPointCount;
        }
    }

    void FinishGame()
    {
        if (gameFinished)
        {
            return;
        }

        gameFinished = true;

        if (winText != null)
        {
            winText.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        JoyconSwingAttack joyconSwingAttack =
            FindObjectOfType<JoyconSwingAttack>();

        if (joyconSwingAttack != null)
        {
            joyconSwingAttack.enabled = false;
        }

        Debug.Log("4つのポイントを集めました。ゲーム終了");
    }
}