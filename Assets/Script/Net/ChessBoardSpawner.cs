using Fusion;
using UnityEngine;

public class ChessSpawner : MonoBehaviour
{
    public NetworkPrefabRef chessBoardPrefab;
    public NetworkRunner runner;

    void Start()
    {
        if (runner.IsServer)
        {
            runner.Spawn(chessBoardPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
