using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField]
    private NetworkObject playerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // 自分が参加したときだけ、自分のプレイヤーを生成する
        if (player == Runner.LocalPlayer)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-3f, 3f),
                1f,
                Random.Range(-3f, 3f)
            );

            Runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }
}