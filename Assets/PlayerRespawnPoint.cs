using UnityEngine;

public class PlayerRespawnPoint : MonoBehaviour
{
    private Vector3 respawnPosition;
    private Quaternion respawnRotation;
    private bool hasRespawnPoint = false;

    public void SetRespawnPoint(Vector3 position, Quaternion rotation)
    {
        respawnPosition = position;
        respawnRotation = rotation;
        hasRespawnPoint = true;

        Debug.Log("RespawnPointを設定しました: " + respawnPosition);
    }

    public void Respawn()
    {
        if (!hasRespawnPoint)
        {
            Debug.LogWarning("RespawnPointが設定されていません");
            return;
        }

        CharacterController controller =
            GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = respawnPosition;
        transform.rotation = respawnRotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Debug.Log("スタート位置に戻りました");
    }
}