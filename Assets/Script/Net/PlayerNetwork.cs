using Fusion;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string Username { get; set; }
    [Networked] public int Team { get; set; } // 0 = White, 1 = Black   
    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            RPC_SetUsername(GameManager.Instance.username);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetUsername(string name)
    {
        Username = name;
    }
}