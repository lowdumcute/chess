using Fusion;
using UnityEngine;
using System.Collections.Generic;

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
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    [Networked] public Vector3 NetworkedPosition { get; set; }

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Start()
    {
        Vector3 desiredForward = (team == 1) ? Vector3.forward : Vector3.back;
        transform.rotation = Quaternion.LookRotation(desiredForward);

        desiredPosition = transform.position;

        if (Object.HasStateAuthority)
        {
            NetworkedPosition = transform.position;
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if ((NetworkedPosition - transform.position).sqrMagnitude > 0.001f)
            {
                NetworkedPosition = transform.position;
            }
        }
        else
        {
            desiredPosition = NetworkedPosition;
        }
    }


    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));
        return r;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> AvaibleMoves)
    {
        return SpecialMove.None;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}
