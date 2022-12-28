using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class Communication
{
    [ServerRpc]
    public static void EndTurnServerRpc()
    {

    }

    [ClientRpc]
    public static void EndTurnClientRpc()
    {

    }

    [ClientRpc]
    public static void StartTurnClientRpc(int id)
    {
        if (id == PlayerGameData.Id)
            PlayerGameData.NewTurn();
    }
    
    [ServerRpc]
    public static void BuildPathServerRpc(BuildPath buildPath, Path path)
    {
        BuildPathClientRpc(buildPath, path);
        UpdatePlayerPointsClientRpc(PlayerGameData.Id, PlayerGameData.curentPoints);
    }

    [ClientRpc]
    public static void BuildPathClientRpc(BuildPath buildPath, Path path)
    {
        buildPath.BuildPathAnimation();
    }

    [ServerRpc]
    public static void UpdatePlayerPointsServerRpc()
    {

    }

    [ServerRpc]
    public static void UpdatePlayerPointsClientRpc(int playerId, int playerPoints)
    {

    }
}
