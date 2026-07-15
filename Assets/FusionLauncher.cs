using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class FusionLauncher : MonoBehaviour
{
    [Header("プレイヤーPrefab")]
    public NetworkObject playerPrefab;

    [Header("ルーム名")]
    public string roomName = "test_room";

    private NetworkRunner runner;

    async void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab が未設定です。NetworkManager の FusionLauncher に Player prefab を入れてください。");
            return;
        }

        await StartGame();
    }

    async Task StartGame()
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        NetworkSceneManagerDefault sceneManager =
            gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError("Photon接続失敗: " + result.ShutdownReason);
            return;
        }

        Debug.Log("Photon接続成功");

        Vector3 spawnPosition = new Vector3(
            Random.Range(-3f, 3f),
            1f,
            Random.Range(-3f, 3f)
        );

        runner.Spawn(
            playerPrefab,
            spawnPosition,
            Quaternion.identity,
            runner.LocalPlayer
        );
    }
}