using System;
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;  
public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

public class ChessBoardNetworkSpawner : NetworkBehaviour
{
    public static ChessBoardNetworkSpawner Instance;
    [SerializeField] private NetworkPrefabRef[] piecePrefabs; // Theo thứ tự enum ChessPieceType
    [SerializeField] private Material[] teamMaterials; // 0 = trắng, 1 = đen
    
    [SerializeField] private float yOffset = 0.0f;

    [SerializeField] private GameObject tilePrefab;
    private GameObject[,] tiles = new GameObject[TILE_COUNT_X, TILE_COUNT_Y];
    [SerializeField] public float tileSize = 1.0f;

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private ChessPiece[,] chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
    public ChessPiece[,] GetChessPieces() => chessPieces;
    private PlayerRef whitePlayer;
    private PlayerRef blackPlayer;
    [Networked] public PlayerRef CurrentTurnPlayer { get; set; }
    public int currentTurnTeam = 0; // 0: đội trắng, 1: đội đen

    private void Awake()
    {
        Instance = this;
    }
    public override void Spawned()
    {
        CurrentTurnPlayer = whitePlayer; // Trắng đi trước
        if (Runner.IsServer)
        {
            // Host (trắng) là player đầu tiên
            whitePlayer = Runner.LocalPlayer;

            // Kiểm tra có đủ 2 người chơi
            if (Runner.ActivePlayers.Count() > 1)
                blackPlayer = Runner.ActivePlayers.ElementAt(1);

            CreateBoardTiles();
            SpawnAllPieces();
        }
    }


    private void SpawnAllPieces()
    {
        int white = 0, black = 1;

        // Quân trắng (bottom)
        SpawnBackRow(0, white);
        SpawnPawns(1, white);

        // Quân đen (top)
        SpawnBackRow(7, black);
        SpawnPawns(6, black);
    }

    private void SpawnBackRow(int row, int team)
    {
        ChessPieceType[] order = {
            ChessPieceType.Rook, ChessPieceType.Knight, ChessPieceType.Bishop,
            team == 0 ? ChessPieceType.Queen : ChessPieceType.King,
            team == 0 ? ChessPieceType.King : ChessPieceType.Queen,
            ChessPieceType.Bishop, ChessPieceType.Knight, ChessPieceType.Rook
        };

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            SpawnSinglePiece(order[x], team, x, row);
        }
    }
    private void CreateBoardTiles()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                GameObject tile = Instantiate(tilePrefab, GetTileCenter(x, y), Quaternion.identity, transform);
                tile.name = $"Tile {x},{y}";
                tiles[x, y] = tile;
            }
        }
    }


    private void SpawnPawns(int row, int team)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            SpawnSinglePiece(ChessPieceType.Pawn, team, x, row);
        }
    }

    private void SpawnSinglePiece(ChessPieceType type, int team, int x, int y)
    {
        Vector3 spawnPos = GetTileCenter(x, y);
        Quaternion rotation = Quaternion.LookRotation(team == 0 ? Vector3.back : Vector3.forward);

        int index = (int)type - 1;
        if (index < 0 || index >= piecePrefabs.Length)
        {
            Debug.LogError($"Invalid piece type or prefab missing for {type} at index {index}");
            return;
        }

        // 🟡 Cấp quyền InputAuthority theo team
        PlayerRef authority = team == 0 ? whitePlayer : blackPlayer;

        NetworkObject obj = Runner.Spawn(
            piecePrefabs[index],
            spawnPos,
            rotation,
            inputAuthority: authority
        );

        if (obj.TryGetComponent(out ChessPiece cp))
        {
            cp.type = type;
            cp.currentX = x;           
            cp.currentY = y; 
            cp.RPC_SetTeam(team);
            chessPieces[x, y] = cp;
        }
    }
    public Vector3 GetTileCenter(int x, int y)
    {
        // Tính offset từ tâm bàn cờ (dựa trên số ô và tileSize)
        float boardWidth = TILE_COUNT_X * tileSize;
        float boardHeight = TILE_COUNT_Y * tileSize;

        Vector3 origin = transform.position - new Vector3(boardWidth, 0, boardHeight) * 0.5f + new Vector3(tileSize, 0, tileSize) * 0.5f;
        return origin + new Vector3(x * tileSize, yOffset, y * tileSize);
    }

    public Material GetMaterialForTeam(int team)
    {
        return teamMaterials[Mathf.Clamp(team, 0, teamMaterials.Length - 1)];
    }
    public GameObject GetTileAt(int x, int y)
    {
        if (x < 0 || x >= TILE_COUNT_X || y < 0 || y >= TILE_COUNT_Y) return null;
        return tiles[x, y];
    }

    public ChessPiece GetPieceAt(int x, int y)
    {
        if (x < 0 || x >= TILE_COUNT_X || y < 0 || y >= TILE_COUNT_Y) return null;
        return chessPieces[x, y];
    }
    
    public void MovePieceOnBoard(ChessPiece piece, int targetX, int targetY)
    {
        chessPieces[piece.currentX, piece.currentY] = null;
        piece.currentX = targetX;
        piece.currentY = targetY;
        chessPieces[targetX, targetY] = piece;
    }
}
