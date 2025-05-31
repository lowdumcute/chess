using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
public class TurnGame : NetworkBehaviour
{
    public static TurnGame Instance;
    [SerializeField] private Image player1;
    [SerializeField] private Image player2;
    public int scorePlayer1 { get; set; }

    public int scorePlayer2 { get; set; }

    [SerializeField] private TMP_Text scorePLayer1Text;
    [SerializeField] private TMP_Text scorePlayer2Text;


    private void Start()
    {
        Instance = this;
        player1.color = Color.green;
        player2.color = Color.white;
    }

    public void AddPointToPlayer(int playerTeam)
    {
        if (playerTeam == 0)
            scorePlayer1++;
        else if (playerTeam == 1)
            scorePlayer2++;

        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        scorePLayer1Text.text = scorePlayer1.ToString();
        scorePlayer2Text.text = scorePlayer2.ToString();
    }

    public void CheckTurn()
    {
        if (ChessBoardNetworkSpawner.Instance.currentTurnTeam == 0)
        {
            player1.color = Color.green;
            player2.color = Color.white;
        }
        else if (ChessBoardNetworkSpawner.Instance.currentTurnTeam == 1)
        {
            player2.color = Color.green;
            player1.color = Color.white;
        }
    }
}
