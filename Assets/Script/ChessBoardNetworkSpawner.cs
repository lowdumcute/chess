using System;
using Fusion;
using UnityEngine;
using System.Linq;
using System.Collections;
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
    [SerializeField] private ChessPiece[,] chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
    public ChessPiece[,] GetChessPieces() => chessPieces;
    [SerializeField] public PlayerRef whitePlayer;
    [SerializeField] public PlayerRef blackPlayer;
    [SerializeField][Networked] public PlayerRef CurrentTurnPlayer { get; set; }
    [Networked, OnChangedRender(nameof(OnTurnChanged))] public int currentTurnTeam { get; set; } // thêm [Networked]

    public override void Spawned()
    {
        Instance = this;
        CreateBoardTiles();

        if (Runner.IsServer)
        {
            whitePlayer = Runner.LocalPlayer; // Host là trắng
            Debug.Log($"[Server] You are White: {whitePlayer}");

            // Chờ đến khi đủ người chơi để lấy blackPlayer
            StartCoroutine(WaitForPlayersAndSpawn());
        }
        else
        {
            // Client: Xác định white/black dựa theo danh sách ActivePlayers
            if (Runner.ActivePlayers.Count() >= 2)
            {
                whitePlayer = Runner.ActivePlayers.ElementAt(0);
                blackPlayer = Runner.ActivePlayers.ElementAt(1);

                if (Runner.LocalPlayer == Runner.ActivePlayers.ElementAt(0))
                {
                    Debug.Log($"[Client] You are White: {Runner.LocalPlayer}");
                }
                else if (Runner.LocalPlayer == Runner.ActivePlayers.ElementAt(1))
                {
                    Debug.Log($"[Client] You are Black: {Runner.LocalPlayer}");
                }

                else
                    Debug.LogWarning($"[Client] Local player không khớp với white hoặc black.");
            }
            else
            {
                Debug.LogWarning("[Client] Không đủ 2 người chơi để gán white và black.");
            }

            StartCoroutine(DelayedRebuild());
        }
    }



    IEnumerator WaitForPlayersAndSpawn()
    {
        // Chờ đến khi đủ 2 player
        while (Runner.ActivePlayers.Count() < 2)
        {
            yield return null;
        }

        // Lấy Player thứ 2
        foreach (var player in Runner.ActivePlayers)
        {
            if (player != whitePlayer)
            {
                blackPlayer = player;
                break;
            }
        }

        SpawnAllPieces(whitePlayer, blackPlayer);
        CurrentTurnPlayer = whitePlayer;
    }


    public void SpawnAllPieces(PlayerRef whitePlayer, PlayerRef blackPlayer)
    {
        int white = 0, black = 1;

        // Quân trắng (bottom)
        SpawnBackRow(0, white, whitePlayer);
        SpawnPawns(1, white, whitePlayer);

        // Quân đen (top)
        SpawnBackRow(7, black, blackPlayer);
        SpawnPawns(6, black, blackPlayer);
    }

    private void SpawnBackRow(int row, int team, PlayerRef playerRef)
    {
        ChessPieceType[] order = {
            ChessPieceType.Rook, ChessPieceType.Knight, ChessPieceType.Bishop,
            team == 0 ? ChessPieceType.Queen : ChessPieceType.King,
            team == 0 ? ChessPieceType.King : ChessPieceType.Queen,
            ChessPieceType.Bishop, ChessPieceType.Knight, ChessPieceType.Rook
        };

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            SpawnSinglePiece(order[x], team, x, row, playerRef);
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


    private void SpawnPawns(int row, int team, PlayerRef playerRef)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            SpawnSinglePiece(ChessPieceType.Pawn, team, x, row, playerRef);
        }
    }

    private void SpawnSinglePiece(ChessPieceType type, int team, int x, int y, PlayerRef authority)
    {
        Vector3 spawnPos = GetTileCenter(x, y);
        Quaternion rotation = Quaternion.LookRotation(team == 0 ? Vector3.back : Vector3.forward);

        int index = (int)type - 1;
        if (index < 0 || index >= piecePrefabs.Length)
        {
            Debug.LogError($"Invalid piece type or prefab missing for {type} at index {index}");
            return;
        }

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
    public void SetPieceAt(int x, int y, ChessPiece piece)
    {
        chessPieces[x, y] = piece;
    }


    public void MovePieceOnBoard(ChessPiece piece, int targetX, int targetY)
    {
        AudioManager.Instance.PlaySFX("Drag", transform.position);
        int oldX = piece.currentX;
        int oldY = piece.currentY;

        // Cập nhật local
        chessPieces[oldX, oldY] = null;
        piece.currentX = targetX;
        piece.currentY = targetY;
        chessPieces[targetX, targetY] = piece;

        // Gửi RPC cho client cập nhật tương ứng
        if (Runner.IsServer)
        {
            RPC_UpdateBoardMap(piece.Id, oldX, oldY, targetX, targetY);
        }
    }
    //phù hợp cho logic gameplay (như bật/tắt AI, trigger hiệu ứng, v.v.).
    private void OnTurnChanged()
    {
        Debug.Log("Turn changed to team: " + currentTurnTeam);
    }
    public void SwitchTurn()
    {
        Debug.Log("Switch Turn");
        currentTurnTeam = 1 - currentTurnTeam;

        if (currentTurnTeam == 0)
            CurrentTurnPlayer = whitePlayer;
        else
            CurrentTurnPlayer = blackPlayer;
    }
    IEnumerator DelayedRebuild()
    {
        yield return new WaitForSeconds(2f);
        RebuildBoardMap();
    }

    void RebuildBoardMap()
    {
        Array.Clear(chessPieces, 0, chessPieces.Length); // Clear cũ
        var all = FindObjectsByType<ChessPiece>(FindObjectsSortMode.None);
        foreach (var piece in all)
        {
            chessPieces[piece.currentX, piece.currentY] = piece;
        }
        Debug.Log($"[Client] Rebuilt board, total: {all.Length}");
    }
    public Vector2Int WorldToBoardCoords(Vector3 worldPos)
    {
        float boardWidth = TILE_COUNT_X * tileSize;
        float boardHeight = TILE_COUNT_Y * tileSize;
        Vector3 origin = transform.position - new Vector3(boardWidth, 0, boardHeight) * 0.5f + new Vector3(tileSize, 0, tileSize) * 0.5f;

        int x = Mathf.FloorToInt((worldPos.x - origin.x + tileSize * 0.5f) / tileSize);
        int y = Mathf.FloorToInt((worldPos.z - origin.z + tileSize * 0.5f) / tileSize);

        x = Mathf.Clamp(x, 0, TILE_COUNT_X - 1);
        y = Mathf.Clamp(y, 0, TILE_COUNT_Y - 1);

        return new Vector2Int(x, y);
    }
    // ăn quân cờ
    public void CapturePiece(int targetX, int targetY)
    {
        var targetPiece = chessPieces[targetX, targetY];
        if (targetPiece != null)
        {
            // Nếu quân bị ăn là vua => kết thúc game
            if (targetPiece.type == ChessPieceType.King)
            {
                int winningTeam = 1 - targetPiece.Team; // Nếu team vua là 0 thì team 1 thắng, ngược lại
                ChessGameManager.Instance.DeclareVictory(winningTeam);
            }
            // Xóa khỏi mảng
            chessPieces[targetX, targetY] = null;

            // Hủy object
            Runner.Despawn(targetPiece.Object);

            // Gửi RPC cho client update
            RPC_UpdateBoardMap_Remove(targetX, targetY);

            Debug.Log($"[Server] Captured piece at ({targetX},{targetY})");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBoardMap_Remove(int x, int y)
    {
        chessPieces[x, y] = null;
        Debug.Log($"[Client] Removed piece at ({x},{y})");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateBoardMap(NetworkBehaviourId pieceId, int oldX, int oldY, int newX, int newY)
    {
        if (Runner.TryFindBehaviour(pieceId, out ChessPiece piece))
        {
            chessPieces[oldX, oldY] = null;
            chessPieces[newX, newY] = piece;
            Debug.Log($"[Client] Updated board: moved {piece.type} from ({oldX},{oldY}) to ({newX},{newY})");
        }
        else
        {
            Debug.LogWarning($"[Client] Could not find piece with id {pieceId}");
        }
    }
    
}
