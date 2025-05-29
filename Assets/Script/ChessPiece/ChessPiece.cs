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
    [Networked] public int Team { get; set;}
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public int currentX { get; set; }
    [Networked] public int currentY { get; set; }
    [Networked] public ChessPieceType type{ get; set; }

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
    // Trong ChessPiece.cs (khi spawn xong)
    public override void Spawned()
    {
        if (Object.HasStateAuthority) // hoặc HasInputAuthority tùy bạn muốn ai quản lý
        {
            ChessBoardNetworkSpawner.Instance.SetPieceAt(currentX , currentY, this);
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
        return new List<Vector2Int> {   };
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
    // RPC gọi từ client lên host/state authority để yêu cầu di chuyển
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestMove(Vector3 targetPosition, RpcInfo info = default)
    {
        // Kiểm tra quyền (nếu muốn), ví dụ chỉ di chuyển khi đúng lượt...
        bool canMove = ChessBoardNetworkSpawner.Instance.currentTurnTeam == Team;
        if (!HasInputAuthority)
        {
            Debug.LogWarning("Client không có quyền điều khiển quân này.");
            return;
        }

        if (canMove)
        {
            Debug.Log($"[RPC_RequestMove] Moving piece {name} to {targetPosition}");

            // Thực hiện di chuyển, cập nhật position cho networked state
            SetPosition(targetPosition, force: true);

            // Nếu cần, có thể gọi thêm các hàm logic khác như cập nhật board...
            ChessBoardNetworkSpawner.Instance.MovePieceOnBoard(this, currentX, currentY); // Cập nhật logic bảng cờ, nếu có

            // Chuyển lượt
            ChessBoardNetworkSpawner.Instance.currentTurnTeam = 1 - ChessBoardNetworkSpawner.Instance.currentTurnTeam;
        }
        else
        {
            Debug.LogWarning($"Player không được phép di chuyển quân cờ này hoặc không đúng lượt.");
        }
    }

}

