using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerPointCounter : NetworkBehaviour
{
    [Header("Point")]
    [SerializeField] private int totalPointCount = 4;

    private HashSet<PointItem> collectedPoints =
        new HashSet<PointItem>();

    public int CurrentPointCount
    {
        get
        {
            return collectedPoints.Count;
        }
    }

    public int TotalPointCount
    {
        get
        {
            return totalPointCount;
        }
    }

    public bool IsAllPointCollected
    {
        get
        {
            return CurrentPointCount >= totalPointCount;
        }
    }

    public void TryGetPoint(PointItem pointItem)
    {
        if (pointItem == null)
        {
            return;
        }

        if (collectedPoints.Contains(pointItem))
        {
            return;
        }

        collectedPoints.Add(pointItem);

        Debug.Log(
            gameObject.name +
            " がポイントを取得: " +
            CurrentPointCount +
            " / " +
            totalPointCount
        );

        PlayerPointTextUI pointUI =
            FindObjectOfType<PlayerPointTextUI>();

        if (pointUI != null)
        {
            pointUI.SetPlayerPointCounter(this);
        }

        if (IsAllPointCollected)
        {
            Debug.Log(gameObject.name + " が4つのポイントを集めました");
        }
    }
}