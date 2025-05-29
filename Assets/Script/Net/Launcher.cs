using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Launcher : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runnerPrefab;
    public NetworkObject chessboardPrefab;
    private PlayerRef whitePlayer;
    private PlayerRef blackPlayer;

    private NetworkRunner runner;

    public void StartAsHost()
    {
        ChessGameManager.Instance.myTeam = 0; // Gán team cho host
        StartGame(GameMode.Host);
    }

    public void StartAsClient()
    {
        ChessGameManager.Instance.myTeam = 1; // Gán team cho client
        StartGame(GameMode.Client);
    }

    async void StartGame(GameMode mode)
    {
        runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && runner.ActivePlayers.Count() == 2)
        {
            Debug.Log("Spawning chessboard...");
            GameUI.Instance.OnGame();

            NetworkObject boardObj = runner.Spawn(chessboardPrefab, Vector3.zero, Quaternion.identity);

            if (boardObj.TryGetComponent(out ChessBoardNetworkSpawner board))
            {
                board.whitePlayer = whitePlayer;
                board.blackPlayer = blackPlayer;
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    #pragma warning disable UNT0006

    public void OnConnectedToServer(NetworkRunner runner)
    {
        if (runner.GameMode == GameMode.Client)
        {
            Debug.Log("Client connected to server. Setting up client camera...");
            GameUI.Instance.OnGame();
            SetCamera.Instance.SetCameraClient();
        }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    #pragma warning restore UNT0006

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
