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

    private static (BuildPath buildPath, Path path) chosenPath;

    public static void ChoosePath(BuildPath buildPath, Path path)
    {
        chosenPath = (buildPath, path);
    }

    public static void ChooseCard(Color color)
    {
        if(chosenPath.buildPath != null && chosenPath.path != null)
        {
            if (chosenPath.path.color == color || chosenPath.path.color == Color.any || color == Color.special)
            {
                //Debug.Log("CanBuildPath" + PlayerGameData.CanBuildPath(chosenPath.path));
                if (PlayerGameData.CanBuildPath(chosenPath.path))
                    BuildPath(chosenPath.buildPath, chosenPath.path);
            }
        }
    }

    public static void BuildPath(BuildPath buildPath, Path path)
    {
        Debug.Log("path: " + path.planetFrom + " " + path.planetTo);
        PlayerGameData.BuildPath(path);
        buildPath.StartCoroutine(buildPath.BuildPathAnimation());
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePlayerPointsServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints);
        EndTurn();
        chosenPath = (null, null);

        Debug.Log("BuildPath");
        Debug.Log("path: " + path.planetFrom + " - " + path.planetTo);
    }

    public static void DrawCard(DrawCardsPanel drawCardsPanel, int index)
    {
        Color color = drawCardsPanel.MoveCard(index);
        PlayerGameData.DrawCard(color);
        if (PlayerGameData.cardsDrewInTurn == 2)
            EndTurn();
    }

    public static void DrawMissions(List<Mission> missions)
    {
        PlayerGameData.DrawMissions(missions);

        Debug.Log("DrawMissions");
        foreach (var mission in missions)
            Debug.Log("missions: " + mission.start + " - " + mission.end);
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
