using UnityEngine;
using System.Collections.Generic;
public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //down
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].Team != Team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }
        
        //up
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
                r.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].Team != Team)
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        //left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].Team != Team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        //right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
                r.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].Team != Team)
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        // top right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].Team != Team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        // top left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].Team != Team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        // bottom right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].Team != Team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        // bottom left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
                r.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].Team != Team)
                    r.Add(new Vector2Int(x, y));
                break;
            }
        }

        return r;
    }
}
