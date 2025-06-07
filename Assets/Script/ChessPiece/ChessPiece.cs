using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
    [Networked] public int Team { get; set; }
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public int currentX { get; set; }
    [Networked] public int currentY { get; set; }
    [Networked] public ChessPieceType type { get; set; }
    private Coroutine moveCoroutine;
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
            ChessBoardNetworkSpawner.Instance.SetPieceAt(currentX, currentY, this);
        }
    }

    private void Update()
    {
        if (!Object.HasStateAuthority)
        {
            transform.position = Vector3.Lerp(transform.position, NetworkedPosition, Time.deltaTime * 30f);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 30f);
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
        return new List<Vector2Int> { };
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
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(AnimateMove(position));
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

        if (canMove)
        {
            Debug.Log($"[RPC_RequestMove] Moving piece {name} to {targetPosition}");

            // Tính lại vị trí ô cờ mới dựa trên targetPosition
            Vector2Int newCoords = ChessBoardNetworkSpawner.Instance.WorldToBoardCoords(targetPosition);
            int targetX = newCoords.x;
            int targetY = newCoords.y;

            // Kiểm tra xem có quân địch ở đó không và ăn
            var targetPiece = ChessBoardNetworkSpawner.Instance.GetPieceAt(targetX, targetY);
            if (targetPiece != null && targetPiece.Team != this.Team)
            {
                ChessBoardNetworkSpawner.Instance.CapturePiece(targetX, targetY);
            }

            // Di chuyển quân cờ
            SetPosition(targetPosition, force: true);
            ChessBoardNetworkSpawner.Instance.MovePieceOnBoard(this, targetX, targetY);

            // Chuyển lượt
            ChessBoardNetworkSpawner.Instance.SwitchTurn();
        }
        else
        {
            Debug.LogWarning($"Player không được phép di chuyển quân cờ này hoặc không đúng lượt.");
        }
    }
    private IEnumerator AnimateMove(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float duration = 0.5f;
        float elapsed = 0f;

        float liftHeight = 1f; // Độ cao nhấc lên

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Lerp vị trí theo dạng parabol: đi lên và hạ xuống
            Vector3 midPos = Vector3.Lerp(startPos, targetPos, t);
            midPos.y += Mathf.Sin(t * Mathf.PI) * liftHeight; // parabol nâng lên

            transform.position = midPos;
            yield return null;
        }

        transform.position = targetPos;
    }

}
