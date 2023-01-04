using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class Communication
{
    public static int mapDataNumber;

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

    public static void DrawCard(DrawCardsPanel drawCardsPanel, int index)
    {
        Color color = drawCardsPanel.MoveCard(index);

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
