using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class Communication
{
    private static GameManager _gameManager;

    private static GameManager _GameManager
    {
        get
        {
            if(_gameManager == null )
                _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            return _gameManager;
        }
    }
    
    
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
        PlayerGameData.BuildPath(path);
        buildPath.StartCoroutine(buildPath.BuildPathAnimation());
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePlayerPointsServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints);
        _GameManager.SetBuildPathDataServerRpc(path.Id);
        EndTurn();
        chosenPath = (null, null);
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
        playerPanel.UpdatePlayersOrderServerRpc();
    }

    public static void StartTurn(int playerId)
    {
        if(playerId == PlayerGameData.Id)
            PlayerGameData.StartTurn();
    }
}
