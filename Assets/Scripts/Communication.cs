using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        //buildPath.StartCoroutine(buildPath.BuildPathAnimation(PlayerGameData.Id));
        buildPath.DoBuildPath(PlayerGameData.Id);
        var playerPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
        playerPanel.UpdatePointsAndSpeceshipsNumServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints, PlayerGameData.spaceshipsLeft);
        _GameManager.SetBuildPathDataServerRpc(path.Id, PlayerGameData.Id);
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
        if (!DrawCardsPanel.IsCardRandom(index) && (Color)drawCardsPanel.actualCardColor[index] == Color.special && PlayerGameData.cardsDrewInTurn == 1)
        {
            _GameManager.SetPopUpWindow("Nie możesz dobrać tej karty!");
            return;
        }

        Color color = drawCardsPanel.MoveCard(index);
        PlayerGameData.DrawCard(color, DrawCardsPanel.IsCardRandom(index));
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

        EndTurn();
    }

    public static void EndTurn()
    {
        PlayerGameData.EndTurn();
        NextTurnActions();
        Debug.Log("EndTurn");

        if (PlayerGameData.spaceshipsLeft < Board.minSpaceshipsLeft)
            PlayerGameData.isLastTurn = true;
    }

    public static void EndAITurn(ArtificialPlayer ai)
    {
        NextTurnActions();
        Debug.Log("EndAiTurn");

        if (ai.spaceshipsLeft < Board.minSpaceshipsLeft)
            ai.isLastTurn = true;
    }

    static void NextTurnActions()
    {
        _PlayerPanel.playersOrderChanged = false;
        _PlayerPanel.UpdatePlayersOrderServerRpc();
        _playerPanel.StartNextPlayerTurnServerRpc();
    }

    public static void StartAiTurn(int id)
    {
        ArtificialPlayer ai = Server.artificialPlayers.Where(ai => ai.Id == id).First();
        if (ai.isLastTurn)
            _GameManager.EndGameServerRpc();
        else
            ai.BestMove();
    }

    public static void StartTurn(int playerId)
    {
        if (playerId == PlayerGameData.Id)
        {
            //PlayerGameData.PrintCards();

            if (PlayerGameData.isLastTurn)
            {
                PlayerGameData.PrintMissions();
                _GameManager.EndGameServerRpc();
            }
            else 
                PlayerGameData.StartTurn();
        }
    }
}
