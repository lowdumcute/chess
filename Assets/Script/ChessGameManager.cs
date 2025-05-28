using UnityEngine;
using Fusion;

public class ChessGameManager : NetworkBehaviour
{
    public static ChessGameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

}
