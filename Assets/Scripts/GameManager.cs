using Assets.GameplayControl;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;

using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour 
{
    public List<Sprite> cardSprites;
    public Sprite missionDoneSprite;
    public GameObject cardButton;
    public GameObject cardBackButton;
    public GameObject cardButtonWithSync;

    private GameObject spawnedCardGameObject;
    public List<GameObject> shipGameObjectList = new();
    public List<TMP_Text> cardStackCounterList = new();
    private Vector3 spaceshipsBase = new(-8, -4, -1);
    public TMP_Text spaceshipCounter;
    private TMP_Text satelliteCounter;

    public bool iSendSpawnCardsServerRpc;
    GameObject drawCardsPanel;
    GameObject cardGoal;
    GameObject drawCardsButton;

    public List<GameObject> spawnedObjects { get; set; } = new List<GameObject>();
    public List<NetworkObject> spawnedSpaceships { get; set; } = new List<NetworkObject>();
    public GameObject endGamePanel;

    PathsPanel pathsPanel;

    void Start()
    {
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        drawCardsPanel = GameObject.Find("DrawCardsPanel");
        cardGoal = GameObject.Find("CardGoal");
        drawCardsButton = GameObject.Find("DrawCardsButton");

        spaceshipCounter = GameObject.Find("SpaceshipCounter").GetComponent<TMP_Text>();
        int index = Server.allPlayersInfo.FindIndex(p => p.Id == PlayerGameData.Id);
        if(index != -1)
            spaceshipCounter.text = Server.allPlayersInfo[index].SpaceshipsLeft.ToString();

        var foundCounter = GameObject.Find("SatelliteCounter");
        if (foundCounter != null)
        {
            satelliteCounter = foundCounter.GetComponent<TMP_Text>();
            satelliteCounter.text = "3";
        }
        else     
        index = Server.allPlayersInfo.FindIndex(p => p.Position == 0);
        if(index != -1)
            SetInfoTextServerRpc($"Tura gracza {Server.allPlayersInfo[index].Name}.");

        ShowStartMessageClientRpc();
        if (!Communication.loadOnStart)
        {
            PlayerInfo nextPlayer = Server.allPlayersInfo.Where(p => p.Position == 0).First();
            if (nextPlayer.IsAI)
                Communication.StartAiTurn(nextPlayer.Id);
        }

        if (NetworkManager != null && !NetworkManager.IsHost)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback +=
                (i) =>
                {
                    Application.Quit();
                };
        }

        if(NetworkManager != null &&  !NetworkManager.IsHost)
        {
            GameObject.Find("Canvas").transform.Find("OptionsMenu").transform.Find("SaveButton").GetComponent<Button>().interactable = false;
        }
    }

    [ClientRpc]
    void ShowStartMessageClientRpc()
    {
        int index = Server.allPlayersInfo.FindIndex(p => p.Position == 0);
        if(index != -1 && Server.allPlayersInfo[index].Id == PlayerGameData.Id)
           ShowFadingPopUpWindow("Początek gry - Twój ruch.");
    }

    List<int> connectedClientIds;

    void Update()
    {
        if (NetworkManager == null) return;

        if(NetworkManager.IsServer)
            if(Server.connectedPlayersCount != NetworkManager.Singleton.ConnectedClients.Count)
            {
                connectedClientIds = new List<int>();
                List<PlayerInfo> allClients = Server.allPlayersInfo.Where(p => p.IsAI == false).ToList();
                PingClientRpc();
                StartCoroutine(VerifyClientIds(allClients));
                Server.connectedPlayersCount = NetworkManager.Singleton.ConnectedClients.Count;
            }
    }

    [ClientRpc]
    void PingClientRpc()
    {
        PingServerRpc(PlayerGameData.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    void PingServerRpc(int id)
    {
        connectedClientIds.Add(id);
    }

    IEnumerator VerifyClientIds(List<PlayerInfo> allClients)
    {
        yield return new WaitForSeconds(1);

        foreach(var player in allClients)
        {
            if(!connectedClientIds.Contains(player.Id))
            {
                var newAI = AddAiWhenClientDisconnected(player.Id);
                if (player.Position == 0)
                    newAI.StartAiTurn();
            }
        }
    }

    ArtificialPlayer AddAiWhenClientDisconnected(int id)
    {
        var playerInfo = Server.allPlayersInfo.First(p => p.Id == id);
        var a = GameObject.Find("Canvas").transform.Find("CardDeck").GetComponent<CardDeck>();
        Dictionary<Color, int> playerNumOfCardsInColor;

        if (a.cardsQuantityPerPlayerPerColor.ContainsKey(id))
        {
            int[] playersCards;
            playersCards = GameObject.Find("Canvas").transform.Find("CardDeck").GetComponent<CardDeck>().cardsQuantityPerPlayerPerColor[playerInfo.Id];

            playerNumOfCardsInColor = new Dictionary<Color, int>();
            for (int i = 0; i < playersCards.Length; i++)
            {
                playerNumOfCardsInColor.Add((Color)i, playersCards[i]);
            }
        }
        else
            playerNumOfCardsInColor = new Dictionary<Color, int>()
            {
                { Color.pink, 1 },
                { Color.red, 1 },
                { Color.blue, 1 },
                { Color.yellow, 1 },
                { Color.green, 1 },
                { Color.special, 1 },
            };

        int playerIndInAllPlayers = -1;
        for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            if (Server.allPlayersInfo[i].Id == playerInfo.Id)
            {
                playerIndInAllPlayers = i;
                break;
            }

        var receivedMissions = GameObject.Find("Canvas").transform.Find("MissionsScroll").transform.Find("PathsPanel").GetComponent<PathsPanel>().receivedMissions;
        var playerMissionData = receivedMissions[playerIndInAllPlayers];
        List<Mission> playerMissions = new List<Mission>(), playerMissionsToDo = new List<Mission>();

        if(playerMissionData != null)
            foreach (var missionData in playerMissionData)
            {
                foreach (var mission in Server.allMissions)
                {
                    if (mission.start.name == missionData.startPlanetName && mission.end.name == missionData.endPlanetName)
                    {
                        playerMissions.Add(mission);
                        playerMissionsToDo.Add(mission);
                    }
                }
            }

        
        playerInfo.IsAI = true;

        var newAI = new ArtificialPlayer
        {
            Id = playerInfo.Id,
            Name = playerInfo.Name,
            curentPoints = playerInfo.Points,
            spaceshipsLeft = playerInfo.SpaceshipsLeft,
            startedLastTurn = playerInfo.SpaceshipsLeft < Board.minSpaceshipsLeft,
            numOfCardsInColor = playerNumOfCardsInColor,
            missions = playerMissions,
            missionsToDo = playerMissionsToDo,
        };
        Server.artificialPlayers.Add(newAI);

        foreach (var mission in newAI.missions)
        SetInfoTextServerRpc($"{playerInfo.Name} rozłączył(a) się i został(a) zastąpiony/a przez komputer.");
        return newAI;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShipsServerRpc(int playerColorNum, Vector3 position, Quaternion rotation,ServerRpcParams serverRpcParams = default)
    {
        float angle = CalculateAngle(position,spaceshipsBase);
        var spawnedShipGameObject = Instantiate(shipGameObjectList[playerColorNum], spaceshipsBase, Quaternion.Euler(new Vector3(0, 0, -angle)));
        spawnedShipGameObject.GetComponent<NetworkObject>().Spawn(true);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.goalPosition = position;
        spawnedShip.goalRotation = rotation;
        spawnedSpaceships.Add(spawnedShipGameObject.GetComponent<NetworkObject>());
    }

    public void SpawnCards(Transform t, int color, string name)
    {
        cardButton.GetComponent<Image>().sprite = cardSprites[color];
        spawnedCardGameObject = Instantiate(cardButton, t);
        spawnedCardGameObject.name = name;
        var spawnedCard = spawnedCardGameObject.GetComponent<Move>();
        spawnedCard.speed = 700;
        Transform cardsStack = GameObject.Find(name + "s").GetComponent<Transform>();
        spawnedCard.goalPosition = cardsStack.position;
        spawnedCard.goalRotation = cardsStack.rotation;
        spawnedObjects.Add(spawnedCardGameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCardsServerRpc(Vector3 position,int color,string name,int index)
    {
       SpawnCardsClientRpc(position, color, name,index);
    }

    [ClientRpc]
    public void SpawnCardsClientRpc(Vector3 position, int color, string name,int index)
    {
        if (!iSendSpawnCardsServerRpc)
        {
            cardButton.GetComponent<Image>().sprite = cardSprites[color];
            if(index!=-1)
                spawnedCardGameObject = Instantiate(cardButton, drawCardsPanel.transform.GetChild(index));
            else
                spawnedCardGameObject = Instantiate(cardBackButton, drawCardsButton.transform);
            spawnedCardGameObject.name = name;
            spawnedCardGameObject.transform.localScale = new(0.5f, 0.5f);
            var spawnedCard = spawnedCardGameObject.GetComponent<Move>();

            spawnedCard.speed = 1000;
            spawnedCard.goalPosition = cardGoal.transform.position;
            spawnedCard.goalRotation = Quaternion.identity;
        }
        else
            iSendSpawnCardsServerRpc = false;
    }

    public float CalculateAngle(Vector3 position1, Vector3 position2)
    {
        return Mathf.Atan2(
                          position1.x - position2.x,
                          position1.y - position2.y
                          ) * Mathf.Rad2Deg;
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetBuildPathDataServerRpc(int pathId, int playerId)
    {
        SetBuildPathDataClientRpc(pathId, playerId);
    }

    [ClientRpc]
    public void SetBuildPathDataClientRpc(int pathId, int playerId)
    {
        Path path = Map.mapData.paths.Where(p => p.Id == pathId).First();
        path.isBuilt = true;
        path.builtById = playerId;
        Map.loadBuildPath[pathId] = true;
    }

    public void SetPopUpWindow(string message)
    {
        var popUp = GameObject.Find("Canvas").transform.Find("PopUpPanel");
        popUp.transform.Find("InfoText").GetComponent<TextMeshProUGUI>().text = message;
        popUp.gameObject.SetActive(true);
    }



    [ServerRpc(RequireOwnership = false)]
    public void LastTurnServerRpc()
    {
        LastTurnClientRpc();
    }

    [ClientRpc]
    public void LastTurnClientRpc()
    {
        Communication.isLastTurn = true;
        ShowFadingPopUpWindow("Ostatnia tura rozpoczyna się.");
    }

    public void ShowFadingPopUpWindow(string message)
    {
        var popUp = GameObject.Find("Canvas").transform.Find("FadingPopUpPanel");
        popUp.transform.Find("InfoText").GetComponent<TMP_Text>().text = message;
        StartCoroutine(ShowFadingPopUpWindowCoroutine(popUp.transform.Find("FadeScript").GetComponent<FadePanel>()));
    }

    IEnumerator ShowFadingPopUpWindowCoroutine(FadePanel fade)
    {
        fade.ShowUp();
        yield return new WaitForSeconds(1.5f);
        fade.FadeOut();
    }

    public void DelayAiMove(ArtificialPlayer ai)
    {
        StartCoroutine(DelayAiMoveCoroutine(ai));
    }

    IEnumerator DelayAiMoveCoroutine(ArtificialPlayer ai)
    {
        yield return new WaitForSeconds(3);
        ai.BestMove();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetInfoTextServerRpc(string text)
    {
        SetInfoTextClientRpc(text);
    }

    [ClientRpc]
    void SetInfoTextClientRpc(string text)
    {
        GameObject.Find("Canvas").transform.Find("InfoText").GetComponent<TextMeshProUGUI>().text = text;
    }

    public void EndAiTurn(ArtificialPlayer ai)
    {
        StartCoroutine(EndAiTurnCoroutine(ai));
    }

    IEnumerator EndAiTurnCoroutine(ArtificialPlayer ai)
    {
        yield return new WaitForSeconds(1);
        Communication.EndAITurn(ai);
    }

    public void EndTurn()
    {
        StartCoroutine(EndTurnCoroutine());
    }

    IEnumerator EndTurnCoroutine()
    {
        yield return new WaitForSeconds(1);
        Communication.EndTurn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRpc()    
    {
        EndGameWithDelayClientRpc();

        foreach (ArtificialPlayer ai in Server.artificialPlayers)
        {
            ai.CalculateFinalPoints();
            SetFinalPointsAndMissionsNumClientRpc(ai.Id, ai.curentPoints, ai.GetMissionIds(), ai.AreMissionsDone());
        }
        
    }

    [ClientRpc]
    public void SetFinalPointsAndMissionsNumClientRpc(int playerId, int playerPoints, int[] missionIds, bool[] isMissionDoneMissionDone)
    {
        var player = Server.allPlayersInfo.Where(p => p.Id == playerId).First();
        player.Points = playerPoints;
        List<Mission> missions = new List<Mission>();
        for(int i = 0; i < missionIds.Length; i++)
        {
            Mission mission = Server.allMissions.First(m => m.id == missionIds[i]);
            mission.isDone = isMissionDoneMissionDone[i];
            missions.Add(mission);
        }

        Server.missionsByPlayerId.Add(playerId, missions);

    }

    [ClientRpc]
    void EndGameWithDelayClientRpc()
    {
        ShowFadingPopUpWindow("Koniec gry!");

        PlayerGameData.CalculateFinalPoints();
        PropagateClientEndDataServerRpc(PlayerGameData.Id, PlayerGameData.curentPoints, 
            PlayerGameData.GetMissionIds(), PlayerGameData.AreMissionsDone());

        StartCoroutine(EndGame());
    }

    [ServerRpc(RequireOwnership = false)]
    void PropagateClientEndDataServerRpc(int id, int finalPoints, int[] missionIds, bool[] areMissionDone)
    {
        SetFinalPointsAndMissionsNumClientRpc(id, finalPoints, missionIds, areMissionDone);
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3);

        DeactivateMap();

        GameObject.Find("Canvas").gameObject.SetActive(false);
        endGamePanel.SetActive(true);
    }

    void DeactivateMap()
    {
        foreach (GameObject gameObject in spawnedObjects)
        {
            if(gameObject != null)
                gameObject.SetActive(false);
        }

        if(NetworkManager.IsServer)
        {
            foreach (NetworkObject gameSpaceship in spawnedSpaceships)
            {
                if (gameSpaceship != null)
                    gameSpaceship.Despawn();
            }
        }
    }

    public void MarkMissionDone(Mission mission)
    {
        GameObject missionButton = GameObject.Find(mission.start.name + "-" + mission.end.name);
        missionButton.transform.GetChild(3).gameObject.SetActive(true);
        missionButton.GetComponent<Image>().sprite = missionDoneSprite;
        SyncMissionsDoneServerRpc(mission.start.name, mission.end.name, PlayerGameData.Id);
    }
    
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncMissionsDoneServerRpc(string missionStartName, string missionEndName, int playerId)
    {
       int index = pathsPanel.receivedMissions[playerId].FindIndex(m => m.startPlanetName == missionStartName && m.endPlanetName == missionEndName);
        if (index != -1)
        {
            pathsPanel.receivedMissions[playerId][index] = new()
            {
                startPlanetName = pathsPanel.receivedMissions[playerId][index].startPlanetName,
                endPlanetName = pathsPanel.receivedMissions[playerId][index].endPlanetName,
                points = pathsPanel.receivedMissions[playerId][index].points,
                isDone = true
            };
        }
    }
}
