using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)

    {
        List<Vector2Int> r = new List<Vector2Int>();
        int direction = (Team == 0) ? 1 : -1;
        // one int fornt 
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));
        // two int fornt
        if (board[currentX, currentY + direction] == null)
        {
            if (currentY == 1 && Team == 0 && board[currentX, currentY + direction * 2] == null)
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
            if (currentY == 6 && Team == 1 && board[currentX, currentY + direction * 2] == null)
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
        }
        // kill move 
        if (currentX != tileCountX - 1)
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].Team != Team)
                r.Add(new Vector2Int(currentX + 1, currentY + direction));
        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].Team != Team)
                r.Add(new Vector2Int(currentX - 1, currentY + direction));

        return r;
    }
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List< Vector2Int> availableMoves)
    {
        int direction = (Team == 0) ? 1 : -1;
        // en Passentt 
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    // check if the last move was a pawn moving two spaces forward
                    if (lastMove[1].y == currentY && Mathf.Abs(lastMove[1].x - currentX) == 1)
                    {
                        if (board[lastMove[1].x, lastMove[1].y].Team != Team)
                        {
                            if (lastMove[1].y == currentY)
                            {
                                // check if the pawn is on the same row and one column to the left or right
                                if (lastMove[1].x == currentY - 1)
                                {
                                    availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                    return SpecialMove.EnPassant;
                                }
                                if (lastMove[1].x == currentX + 1)
                                {
                                    availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                    return SpecialMove.EnPassant;
                                }
                            }

                        }
                    }
                }
            }



        }

        return SpecialMove.None;
    }
}
