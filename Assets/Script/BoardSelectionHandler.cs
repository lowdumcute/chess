using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BoardSelectionHandler : MonoBehaviour
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material moveHighlightMaterial;

    private Camera currentCamera;
    private GameObject currentHoveredTile;
    private ChessPiece selectedPiece;
    private List<Vector2Int> availableMoves = new();
    private List<GameObject> moveIndicators = new();

    private void Update()
    {
        if (currentCamera == null)
        {
            currentCamera = Camera.main;
            return;
        }

        HandleHoverAndClick();
    }

    void HandleHoverAndClick()
    {
        Ray ray = currentCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            GameObject tile = hit.collider.gameObject;
            if (currentHoveredTile != tile)
            {
                ResetHover();

                currentHoveredTile = tile;
                currentHoveredTile.layer = LayerMask.NameToLayer("Hover");

                SetMaterial(currentHoveredTile, highlightMaterial);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2Int coord = GetTileCoord(tile.transform.position);
                ChessPiece clickedPiece = ChessBoardNetworkSpawner.Instance.GetPieceAt(coord.x, coord.y);
                Debug.Log($"was clicked in {clickedPiece},");

                if (clickedPiece != null )
                {
                    Debug.Log("SelectPiece");
                    SelectPiece(clickedPiece, coord);
                    
                }
                    else if (selectedPiece != null && availableMoves.Contains(coord))
                    {
                        MoveSelectedPieceTo(coord);
                    }
                    else if (clickedPiece == null)
                    {
                        Debug.Log("đéo có quân cờ nào");
                    }
            }
        }
        else
        {
            ResetHover();
        }
    }

    void SelectPiece(ChessPiece piece, Vector2Int coord)
    {
        selectedPiece = piece;
        availableMoves.Clear();
        ClearMoveHighlights();

        var board = ChessBoardNetworkSpawner.Instance.GetChessPieces();
        int boardSizeX = 8;
        int boardSizeY = 8;

        availableMoves = selectedPiece.GetAvailableMoves(ref board, boardSizeX, boardSizeY);

        foreach (var move in availableMoves)
        {
            GameObject tile = ChessBoardNetworkSpawner.Instance.GetTileAt(move.x, move.y);
            if (tile != null)
            {
                tile.layer = LayerMask.NameToLayer("Highlight");
                SetMaterial(tile, moveHighlightMaterial);
                moveIndicators.Add(tile);
            }
        }
    }

    void MoveSelectedPieceTo(Vector2Int targetCoord)
    {
        Debug.Log($"Requesting move of piece {selectedPiece.name} to {targetCoord}");

        Vector3 targetPos = ChessBoardNetworkSpawner.Instance.GetTileCenter(targetCoord.x, targetCoord.y);

        // Gọi RPC từ client lên server/state authority để yêu cầu di chuyển
        if (selectedPiece.HasInputAuthority)
        {
            selectedPiece.RPC_RequestMove(targetPos);
        }
        else
        {
            Debug.LogWarning("Client không có quyền gửi yêu cầu di chuyển quân cờ này.");
        }

        // Việc đó sẽ do bên StateAuthority xử lý trong RPC_RequestMove

        selectedPiece = null;
        availableMoves.Clear();
        ClearMoveHighlights();
    }


    void SetMaterial(GameObject obj, Material mat)
    {
        if (obj.TryGetComponent<MeshRenderer>(out var renderer))
        {
            renderer.material = mat;
        }
    }

    void ResetHover()
    {
        if (currentHoveredTile != null)
        {
            Vector2Int coord = GetTileCoord(currentHoveredTile.transform.position);
            if (availableMoves.Contains(coord))
                currentHoveredTile.layer = LayerMask.NameToLayer("Highlight");
            else
                currentHoveredTile.layer = LayerMask.NameToLayer("Tile");

            SetMaterial(currentHoveredTile, defaultMaterial);
            currentHoveredTile = null;
        }
    }

    void ClearMoveHighlights()
    {
        foreach (var tile in moveIndicators)
        {
            tile.layer = LayerMask.NameToLayer("Tile");
            SetMaterial(tile, defaultMaterial);
        }
        moveIndicators.Clear();
    }

    Vector2Int GetTileCoord(Vector3 pos)
    {
        float size = ChessBoardNetworkSpawner.Instance.tileSize;
        Vector3 boardOrigin = ChessBoardNetworkSpawner.Instance.transform.position - new Vector3(8 * size, 0, 8 * size) * 0.5f + new Vector3(size, 0, size) * 0.5f;

        int x = Mathf.FloorToInt((pos - boardOrigin).x / size);
        int y = Mathf.FloorToInt((pos - boardOrigin).z / size);
        return new Vector2Int(x, y);
    }
}
