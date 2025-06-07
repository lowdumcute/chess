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

    public NetworkRunner runner;

    public void StartAsHost()
    {
        ChessGameManager.Instance.myTeam = 0;

        // Tạo mã phòng ngẫu nhiên
        string roomCode = GenerateRoomCode(6);
        Debug.Log($"[HOST] Room code: {roomCode}");
        GUIUtility.systemCopyBuffer = roomCode; // Tự động copy

        StartGame(GameMode.Host, roomCode);
    }

    public void StartAsClient()
    {
        ChessGameManager.Instance.myTeam = 1;

        string roomCode = GameUI.Instance?.GetAddressInput();
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogWarning("Room code is empty! Please enter a code to join.");
            return;
        }

        StartGame(GameMode.Client, roomCode);
    }

    async void StartGame(GameMode mode, string roomCode)
    {
        runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomCode,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    public void SpawnBoard()
    {
        runner.Spawn(chessboardPrefab, Vector3.zero, Quaternion.identity);
    }

    string GenerateRoomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} joined.");

        if (runner.IsServer && runner.ActivePlayers.Count() == 2)
        {
            Debug.Log("Spawning chessboard...");
            GameUI.Instance.OnGame();

            NetworkObject boardObj = runner.Spawn(chessboardPrefab, Vector3.zero, Quaternion.identity);
            if (boardObj.TryGetComponent(out ChessBoardNetworkSpawner board))
            {
                board.whitePlayer = runner.ActivePlayers.First();
                board.blackPlayer = runner.ActivePlayers.Last();
            }
        }
    }

    // Các callback còn lại giữ nguyên
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    #pragma warning disable UNT0006
    public void OnConnectedToServer(NetworkRunner runner)
    {
        if (runner.GameMode == GameMode.Client)
        {
            Debug.Log("Client connected to server.");
            GameUI.Instance.OnGame();
            SetCamera.Instance.SetCameraClient();
        }
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected from server. Reason: " + reason);
        
        // Quay lại menu chính khi đã ngắt kết nối
        GameUI.Instance.OnOnlineBackButton();
    }

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
