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
    
    public static void BuildPath(BuildPath buildPath, Path path)
    {
        buildPath.StartCoroutine(buildPath.BuildPathAnimation());
        PlayerGameData.BuildPath(path);
        UpdatePlayerPointsClientRpc(PlayerGameData.Id, PlayerGameData.curentPoints);
    }

    [ServerRpc]
    public static void DrawCard(Color color)
    {
        // rozpoczecie animacji

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
