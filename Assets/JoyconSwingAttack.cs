using System.Collections.Generic;
using UnityEngine;

public class JoyconSwingAttack : MonoBehaviour
{
    public SwordAttack swordAttack;

    public float swingThreshold = 8.0f;
    public float attackInterval = 0.7f;

    private List<Joycon> joycons;
    private float lastAttackTime = -10.0f;

    void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (swordAttack == null)
        {
            swordAttack = GetComponent<SwordAttack>();
        }

        if (joycons == null || joycons.Count == 0)
        {
            Debug.LogWarning("Joy-Conが見つかりません。Jキー攻撃はそのまま使えます。");
        }
        else
        {
            Debug.Log("Joy-Con振り攻撃準備完了");
        }
    }

    void Update()
    {
        if (joycons == null || joycons.Count == 0 || swordAttack == null)
        {
            return;
        }

        Joycon joycon = joycons[0];

        Vector3 gyro = joycon.GetGyro();

        float swingPower = gyro.magnitude;

        if (swingPower > swingThreshold && Time.time - lastAttackTime > attackInterval)
        {
            lastAttackTime = Time.time;

            Debug.Log("Joy-Con振り攻撃 / power: " + swingPower);

            swordAttack.TryAttack();
        }
    }
}