using UnityEngine;
using Fusion;

public class PromotionUI : MonoBehaviour
{
    public static PromotionUI Instance;
    public GameObject PromotionPanel;
    private ChessPiece pendingPawn;

    private void Awake()
    {
        Instance = this;
        PromotionPanel.SetActive(false);
    }

    public void Show(ChessPiece pawn)
    {
        pendingPawn = pawn;
        PromotionPanel.SetActive(true);
    }

    public void ChooseQueen() => Select(ChessPieceType.Queen);
    public void ChooseRook() => Select(ChessPieceType.Rook);
    public void ChooseBishop() => Select(ChessPieceType.Bishop);
    public void ChooseKnight() => Select(ChessPieceType.Knight);

    void Select(ChessPieceType type)
    {
        if (pendingPawn != null)
        {
            pendingPawn.RPC_RequestPromotion(type);
        }

        PromotionPanel.SetActive(false);
        pendingPawn = null;
    }
}