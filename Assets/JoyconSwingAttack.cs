using System.Collections.Generic;
using UnityEngine;

public class JoyconSwingAttack : MonoBehaviour
{
    public PlayerMovement playerMovement;

    [Header("Joy-Con")]
    public int joyconIndex = 0;

    [Header("Swing Settings")]
    public float swingThreshold = 4.0f;
    public float attackInterval = 0.7f;

    [Header("Direction Judge")]
    public float directionDifference = 1.2f;

    private List<Joycon> joycons;
    private float lastAttackTime = -10.0f;

    void Start()
    {
        Debug.Log("JoyconSwingAttack Start");

        if (playerMovement == null)
        {
            FindPlayerMovement();
        }
    }

    void Update()
    {
        if (playerMovement == null || !playerMovement.gameObject.activeInHierarchy)
        {
            FindPlayerMovement();
        }

        if (JoyconManager.Instance == null)
        {
            return;
        }

        joycons = JoyconManager.Instance.j;

        if (joycons == null || joycons.Count == 0)
        {
            return;
        }

        if (joyconIndex < 0 || joyconIndex >= joycons.Count)
        {
            return;
        }

        if (playerMovement == null)
        {
            return;
        }

        Joycon joycon = joycons[joyconIndex];

        Vector3 gyro = joycon.GetGyro();

        float xPower = Mathf.Abs(gyro.x);
        float yPower = Mathf.Abs(gyro.y);
        float zPower = Mathf.Abs(gyro.z);

        float swingPower = Mathf.Max(xPower, yPower, zPower);

        if (swingPower < swingThreshold)
        {
            return;
        }

        if (Time.time - lastAttackTime < attackInterval)
        {
            return;
        }

        lastAttackTime = Time.time;

        Debug.Log(
            "Joy-Con Swing / x: " + xPower +
            " / y: " + yPower +
            " / z: " + zPower
        );

        if (xPower > yPower * directionDifference &&
            xPower > zPower * directionDifference)
        {
            Debug.Log("Joy-Con 縦振り → Jキー動作：フル攻撃");
            playerMovement.RequestFullAttack();
        }
        else if (yPower > xPower * directionDifference &&
                 yPower > zPower * directionDifference)
        {
            Debug.Log("Joy-Con 横振り → Kキー動作：刺す攻撃");
            playerMovement.RequestStabAttack();
        }
        else
        {
            Debug.Log("Joy-Con 振り方向が曖昧 → フル攻撃");
            playerMovement.RequestFullAttack();
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
}