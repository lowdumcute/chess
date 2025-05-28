using System.Collections.Generic;
using UnityEngine;

public static class GameRules
{
    public static List<Vector2Int> GetValidMoves(ChessPiece piece, ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        if (piece == null)
            return new List<Vector2Int>();

        List<Vector2Int> moves = piece.GetAvailableMoves(ref board, tileCountX, tileCountY);

        // Bạn có thể lọc chiếu ở đây (nếu đã có logic chiếu)
        // return FilterLegalMoves(piece, moves, board);

        return moves;
    }

    public static bool IsValidMove(ChessPiece piece, int x, int y, ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        var validMoves = GetValidMoves(piece, board, tileCountX, tileCountY);
        return validMoves.Contains(new Vector2Int(x, y));
    }
}
