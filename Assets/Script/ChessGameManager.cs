using UnityEngine;
using Fusion;
using TMPro;
public class ChessGameManager : NetworkBehaviour

{
    public static ChessGameManager Instance;
    [SerializeField] private Launcher launcher; // Tham chiếu đến Launcher để reset game
    public GameObject ScreenResult;
    public TMP_Text ResultText; // Hiển thị kết quả trên UI
    public int myTeam; // 0 hoặc 1, gán từ Launcher hoặc UI

    private void Awake()
    {
        ScreenResult.SetActive(false);
        ResultText.text = "";
        Instance = this;
    }
    public void DeclareVictory(int winningTeam)
    {
        Debug.Log($"[Server] Team {winningTeam} wins!");

        // Gửi RPC thông báo client hiển thị kết quả
        RPC_ShowVictory(winningTeam);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowVictory(int winningTeam)
    {
        TurnGame.Instance.AddPointToPlayer(winningTeam);
        if (winningTeam == myTeam)
        {
            ShowVictoryScreen();
        }
        else
        {
            ShowDefeatScreen();
        }

    }
    public void ShowVictoryScreen()
    {
        Debug.Log("Victory! You win!");
        ScreenResult.SetActive(true);
        ResultText.text = "Victory! You win!";
    }
    public void ShowDefeatScreen()
    {
        Debug.Log("Defeat! You lose!");
        ScreenResult.SetActive(true);
        ResultText.text = "Defeat! You lose!";
    }
    public void ResetGame()
    {
        ScreenResult.SetActive(false);
        if (!Runner.IsServer) return; // Chỉ server mới có quyền reset
        ChessBoardNetworkSpawner.Instance.ResetBoard();
        launcher.SpawnBoard();
    }
    public void KickPlayer()
    {
        if (!Object.HasStateAuthority) return; // Chỉ host có quyền

        Runner.Disconnect(ChessBoardNetworkSpawner.Instance.blackPlayer); // Ngắt kết nối client này
    }

}
