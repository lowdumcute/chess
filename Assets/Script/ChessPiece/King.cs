using UnityEngine;
using System.Collections.Generic;
public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // right
        if (currentX + 1 < tileCountX)
        {
            if (board[currentX + 1, currentY] == null)
                r.Add(new Vector2Int(currentX + 1, currentY));
            else if (board[currentX + 1, currentY].Team != Team)
                r.Add(new Vector2Int(currentX + 1, currentY));

            // top right
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX + 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (board[currentX + 1, currentY + 1].Team != Team)
                    r.Add(new Vector2Int(currentX + 1, currentY + 1));
            }
            // bottom right
            if (currentY - 1 >= 0)
            {
                if (board[currentX + 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (board[currentX + 1, currentY - 1].Team != Team)
                    r.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
        }
        // left
        if (currentX - 1 >= 0)
        {
            if (board[currentX - 1, currentY] == null)
                r.Add(new Vector2Int(currentX - 1, currentY));
            else if (board[currentX - 1, currentY].Team != Team)
                r.Add(new Vector2Int(currentX - 1, currentY));

            // top left
            if (currentY + 1 < tileCountY)
            {
                if (board[currentX - 1, currentY + 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (board[currentX - 1, currentY + 1].Team != Team)
                    r.Add(new Vector2Int(currentX - 1, currentY + 1));
            }
            // bottom left
            if (currentY - 1 >= 0)
            {
                if (board[currentX - 1, currentY - 1] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (board[currentX - 1, currentY - 1].Team != Team)
                    r.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
        }
        
        // up
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX, currentY + 1] == null)
                r.Add(new Vector2Int(currentX, currentY + 1));
            else if (board[currentX, currentY + 1].Team != Team)
                r.Add(new Vector2Int(currentX, currentY + 1));
        }

        // down
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null)
                r.Add(new Vector2Int(currentX, currentY - 1));
            else if (board[currentX, currentY - 1].Team != Team)
                r.Add(new Vector2Int(currentX, currentY - 1));
        }

        return r;
    }
}
