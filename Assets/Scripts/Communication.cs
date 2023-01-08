using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    private static PlayerPanel _playerPanel;
    private static PlayerPanel _PlayerPanel
    {
        get
        {
            if (_playerPanel == null)
                _playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
            return _playerPanel;
        }
    }
    public static int mapDataNumber;
    private static bool isLastTurn = false;


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
                else
                    _GameManager.SetPopUpWindow("Nie możnesz wybudować tej ścieżki!");
            }
        }
    }

    public static void BuildPath(BuildPath buildPath, Path path)
    {
        if (!PlayerGameData.isNowPlaying)
        {
            SetNotThisTurnPopUpWindow();
            return;
        }
        
        PlayerGameData.BuildPath(path);
        Debug.Log("CurrentPoints: " + PlayerGameData.curentPoints);

        buildPath.StartCoroutine(buildPath.BuildPathAnimation());
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePointsAndSpeceshipsNumServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints, PlayerGameData.spaceshipsLeft);
        _GameManager.SetBuildPathDataServerRpc(path.Id);
        EndTurn();
        chosenPath = (null, null);
        
    }

    public static void SetNotThisTurnPopUpWindow()
    {
        _GameManager.SetPopUpWindow("Poczekaj na swoją turę!");
    }

    public static void DrawCard(DrawCardsPanel drawCardsPanel, int index)
    {
        if (!PlayerGameData.isNowPlaying)
        {
            SetNotThisTurnPopUpWindow();
            return;
        }
        
        Color color = drawCardsPanel.MoveCard(index);
        PlayerGameData.DrawCard(color);
        if (PlayerGameData.cardsDrewInTurn == 2)
            EndTurn();
    }

    public static void DrawMissions(List<Mission> missions)
    {
        if (!PlayerGameData.isNowPlaying)
        {
            SetNotThisTurnPopUpWindow();
            return;
        }

        PlayerGameData.DrawMissions(missions);

        Debug.Log("DrawMissions");
        foreach (var mission in missions)
            Debug.Log("missions: " + mission.start + " - " + mission.end);
    }

    public static void EndTurn()
    {
        PlayerGameData.EndTurn();
        Debug.Log("Before order update: ");
        foreach (var player in Server.allPlayersInfo)
        {
            Debug.Log("name: " + player.Name + " posiotion: " + player.Position);
        }
        _PlayerPanel.UpdatePlayersOrderServerRpc();
        Debug.Log("After order update: ");
        foreach (var player in Server.allPlayersInfo)
        {
            Debug.Log("name: " + player.Name + " posiotion: " + player.Position);
        }
        _playerPanel.StartNextPlayerTurnServerRpc();
        Debug.Log("EndTurn");

        if (isLastTurn)
        {
            PlayerGameData.PrintMissions();
            
            _GameManager.EndGameServerRpc();
        }

        if (PlayerGameData.spaceshipsLeft < Board.minSpaceshipsLeft)
            isLastTurn = true;

    }

    public static void EndAITurn(ArtificialPlayer ai)
    {
        _PlayerPanel.UpdatePlayersOrderServerRpc();
        _playerPanel.StartNextPlayerTurnServerRpc();
        Debug.Log("EndAiTurn");

        if (isLastTurn)
            _GameManager.EndGameServerRpc();

        if (ai.spaceshipsLeft < Board.minSpaceshipsLeft)
            isLastTurn = true;
    }

    public static void StartTurn(int playerId)
    {
        if(playerId == PlayerGameData.Id)
            PlayerGameData.StartTurn();
    }
}
