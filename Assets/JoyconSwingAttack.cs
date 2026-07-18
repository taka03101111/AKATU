using System.Collections.Generic;
using UnityEngine;

public class JoyconSwingAttack : MonoBehaviour
{
    public PlayerMovement playerMovement;

    public float swingThreshold = 3.0f;
    public float attackInterval = 0.7f;

    public Joycon.Button aButton = Joycon.Button.DPAD_RIGHT;

    private List<Joycon> joycons;
    private float lastAttackTime = -10.0f;
    private float logTimer = 0.0f;

    void Start()
    {
        Debug.Log("JoyconSwingAttack Start できてる");
    }

    void Update()
    {
        if (playerMovement == null || !playerMovement.gameObject.activeInHierarchy)
        {
            FindPlayerMovement();
        }

        logTimer += Time.deltaTime;

        if (logTimer >= 1.0f)
        {
            logTimer = 0.0f;
            Debug.Log("JoyconSwingAttack Update 動いてる");
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

        if (playerMovement == null)
        {
            return;
        }

        Joycon joycon = joycons[0];

        Vector3 gyro = joycon.GetGyro();
        Vector3 accel = joycon.GetAccel();

        float swingPower = gyro.magnitude + accel.magnitude;

        if (swingPower > swingThreshold &&
            Time.time - lastAttackTime > attackInterval)
        {
            lastAttackTime = Time.time;

            bool isAPressed = joycon.GetButton(aButton);

            if (isAPressed)
            {
                Debug.Log("Joy-Con A + 振り → Kキー動作：刺す攻撃");
                playerMovement.RequestStabAttack();
            }
            else
            {
                Debug.Log("Joy-Con振り → Jキー動作：フル攻撃");
                playerMovement.RequestFullAttack();
            }
        }
    }

    void FindPlayerMovement()
    {
        PlayerMovement[] players =
            FindObjectsOfType<PlayerMovement>();

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

        if (players.Length > 0)
        {
            playerMovement = players[0];
            Debug.Log("PlayerMovementを仮で設定しました: " + players[0].name);
        }
    }
}