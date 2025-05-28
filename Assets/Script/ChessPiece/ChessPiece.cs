using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Fusion.Analyzer;
public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class ChessPiece : NetworkBehaviour
{
    public int Team;
    [Networked]
    public Vector3 NetworkedPosition { get; set; }
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        Vector3 desiredForward = (Team == 1) ? Vector3.forward : Vector3.back;
        transform.rotation = Quaternion.LookRotation(desiredForward);

        if (Object.HasStateAuthority)
        {
            NetworkedPosition = transform.position;
        }
    }

    private void Update()
    {
        if (!Object.HasStateAuthority)
        {
            transform.position = Vector3.Lerp(transform.position, NetworkedPosition, Time.deltaTime * 15f);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10f);
    }


    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Cập nhật vị trí hiện tại lên network
            if ((transform.position - NetworkedPosition).sqrMagnitude > 0.0001f)
            {
                NetworkedPosition = transform.position;
            }
        }
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        return new List<Vector2Int> {
            new Vector2Int(3, 3),
            new Vector2Int(3, 4),
            new Vector2Int(4, 3),
            new Vector2Int(4, 4)
        };
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> AvaibleMoves)
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        if (Object.HasStateAuthority)
        {
            NetworkedPosition = position;
        }

        if (force)
        {
            transform.position = position; // Cho host và client đều cập nhật
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = scale;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetTeam(int team)
    {
        Team = team;

        if (TryGetComponent<MeshRenderer>(out var renderer))
        {
            Material mat = ChessBoardNetworkSpawner.Instance?.GetMaterialForTeam(team);
            if (mat != null)
                renderer.material = new Material(mat);
            else
                Debug.LogWarning("Material không tồn tại hoặc ChessBoardNetworkSpawner bị null.");
        }

        Vector3 desiredForward = (team == 1) ? Vector3.forward : Vector3.back;
        transform.rotation = Quaternion.LookRotation(desiredForward);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_MoveTo(Vector3 targetPosition)
    {
        Debug.Log($"[RPC] Piece {name} moved to {targetPosition}");
        SetPosition(targetPosition, force: true);
    }


}

