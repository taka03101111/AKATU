using System.Collections.Generic;
using UnityEngine;

public class JoyconLibCheck : MonoBehaviour
{
    private List<Joycon> joycons;
    private float timer = 0.0f;

    void Start()
    {
        joycons = JoyconManager.Instance.j;

        if (joycons == null)
        {
            Debug.LogWarning("Joy-Conリストがnullです");
            return;
        }

        Debug.Log("Joy-Conの数: " + joycons.Count);
    }

    void Update()
    {
        if (joycons == null || joycons.Count == 0)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= 1.0f)
        {
            timer = 0.0f;

            Joycon joycon = joycons[0];

            Vector3 accel = joycon.GetAccel();
            Vector3 gyro = joycon.GetGyro();

            Debug.Log("accel: " + accel + " / gyro: " + gyro);
        }
    }
}