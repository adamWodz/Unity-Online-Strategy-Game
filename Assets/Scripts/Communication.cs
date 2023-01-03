using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class Communication
{
    [ClientRpc]
    public static void StartTurnClientRpc(int id)
    {
        if (id == PlayerGameData.Id)
            PlayerGameData.StartTurn();
    }
    
    public static void BuildPath(BuildPath buildPath, Path path)
    {
        Debug.Log("CanBuildPath" + PlayerGameData.CanBuildPath(path));
        if (!PlayerGameData.CanBuildPath(path)) return;
        PlayerGameData.BuildPath(path);
        buildPath.StartCoroutine(buildPath.BuildPathAnimation());
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePlayerPointsServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints);
        EndTurn();
    }

    public static void DrawCard(DrawCardsPanel drawCardsPanel, int index)
    {
        Color color = drawCardsPanel.MoveCard(index);
        PlayerGameData.DrawCard(color);
        if (PlayerGameData.cardsDrewInTurn == 2)
            EndTurn();
    }

    public static void EndTurn()
    {
        PlayerGameData.EndTurn();
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePlayersOrderClientRpc();
    }

    public static void StartTurn()
    {
        PlayerGameData.StartTurn();
    }
}
