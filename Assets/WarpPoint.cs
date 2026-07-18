using System.Collections;
using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    public Transform warpDestination;
    public float warpCoolTime = 1.0f;

    private bool canWarp = true;

    void OnTriggerEnter(Collider other)
    {
        if (!canWarp)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            StartCoroutine(Warp(other.transform));
        }
    }

    IEnumerator Warp(Transform player)
    {
        canWarp = false;

        if (warpDestination == null)
        {
            Debug.LogWarning("Warp Destinationが設定されていません");
            canWarp = true;
            yield break;
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.position = warpDestination.position;
        player.rotation = warpDestination.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log("一方通行ワープしました");

        yield return new WaitForSeconds(warpCoolTime);

        canWarp = true;
    }
}