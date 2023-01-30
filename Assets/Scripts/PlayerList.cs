using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;

public class PlayerList : MonoBehaviour
{
    public MainMenu mainMenu;
    public GameObject createGameMenu;

    public List<PlayerSeat> seats;
    public LobbyAndRelay lobbyData;
    public Text joined, remaining;
    
    private int joinedNum;
    public bool pauseRefresh = false;


    private void Start()
    {
        RefreshList();
    }

    private void Update()
    {
    }

    void RefreshNum()
    {
        ShowCurrentNums();
    }

    public void PauseRefresh()
    {
        pauseRefresh = true;
    }
    public void ContinueRefresh()
    {
        pauseRefresh = false;
    }

    public void RefreshList()
    {
        try
        {
            ShowCurrentPlayers();
            if (lobbyData.joinedLobby != null)
            {
                if (!lobbyData.ImInLobby())
                {
                    mainMenu.gameObject.SetActive(true);
                    createGameMenu.SetActive(false);
                }
            }
            
        }
        catch (LobbyServiceException e)
        {
        }
    }

    private void SetPlayerNick(int j, string pref, string playerId)
    {
        string id;
        if (playerId.Length >= 5) id = playerId.Substring(0, 5);
        else id = playerId;
        seats[j].Nickname.text = $"@ {pref}_{id}";
    }

    private void SetAINicknames(string indexes)
    {
        int a = 1;
        foreach(var i in indexes)
        {
            SetPlayerNick(i, "AIPlayer", (a++).ToString());
        }
    }

    public void NonAI()
    {
        int res = 0, i=0;

        RefreshAIPlayers();
        lobbyData.AISeats = IndexesAI;
    
        for (int j = 0; j < seats.Count; j++) seats[j].PlayerType.interactable = false;
    }


    public enum PlayerListState { EditTypes, CreateLobby, PlayersJoining }
    public string IndexesAI, IndexesReg;
    PlayerListState State;

    public void RefreshAIPlayers()
    {
        if (lobbyData.joinedLobby == null)
        {
            IndexesAI = "";
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].AI) IndexesAI += i.ToString();
                else seats[i].Nickname.text = "";
            }
        }
        else
        {
            IndexesAI = lobbyData.joinedLobby.Data["IndexesAI"].Value;
        }

        int a = 1;
        foreach (char j in IndexesAI)
        {
            var jj = int.Parse(j.ToString());
            if (jj > 0)
            {
                if (!seats[jj].AI) seats[jj].ChangePlayerType();
                seats[jj].Nickname.text = $"{Assets.GameplayControl.PlayerGameData.AINames[a - 1]}";
                a++;
            }
        }
    }

    private void DisableTypeButtons()
    {
        for (int j = 0; j < seats.Count; j++) seats[j].PlayerType.interactable = false;
    }
    public void OnCreateLobby()
    {
        RefreshAIPlayers();
        DisableTypeButtons();
        lobbyData.AISeats = IndexesAI;
        lobbyData.maxPlayers = 5 - IndexesAI.Length;
    }
    public void RefreshRegPlayers()
    {
        if (lobbyData != null && lobbyData.joinedLobby!=null)
        {
            var oldIndexesReg = IndexesReg;
            IndexesReg = "";
            var host = lobbyData.ImLobbyHost();
            var lobby = lobbyData.joinedLobby;
            var players = lobby.Players;

            var indexesAI = IndexesAI;
            var data = lobby.Data;
            if (data != null)
            {
                if (data.ContainsKey("IndexesAI")) indexesAI = data["IndexesAI"].Value;
            }

            var foundLobby = GameObject.Find("LobbyAndRelay");
            if (lobby != null && foundLobby != null)
            {
                var lobby2 = foundLobby.GetComponent<LobbyAndRelay>();
                if (lobby2.joinedLobby != null) 
                players = lobby2.joinedLobby.Players;
            }
            else      

            if (players == null || players.Count > 0)
            {
                int r = 0;
                bool freeSeat, playersLeft;
                for (int i = 0; i < seats.Count; i++)
                {
                    if (players.Count == IndexesReg.Length) break;
                    freeSeat = !indexesAI.Contains(i.ToString()) && !IndexesReg.Contains(i.ToString());
                    if (freeSeat) IndexesReg += i.ToString();
                }

                r = 0;
                foreach (char j in IndexesReg)
                {
                    playersLeft = r < players.Count;
                    if (playersLeft)
                    {
                        
                        var userData = players[r].Data;
                        string username = "Gracz";
                        if (userData != null)
                        {
                            username = userData["UserName"].Value;
                        }
                        else
                        {
                            if (!lobbyData.ImLobbyHost()) username = Assets.GameplayControl.PlayerGameData.Name;
                        }

                        var jj = int.Parse(j.ToString());
                        if (players[r].Id == Unity.Services.Authentication.AuthenticationService.Instance.PlayerId)
                        {
                        }

                        seats[jj].playerId = players[r++].Id;
                        seats[jj].Nickname.text = username;
                        seats[jj].DisplayJoined(false);
                    }
                }

                if (oldIndexesReg.Length > IndexesReg.Length)
                {
                    foreach (char j in oldIndexesReg)
                    {
                        if (!IndexesReg.Contains(j.ToString()))
                        {
                            var jj = int.Parse(j.ToString());
                            seats[jj].FreeSeat();
                        }
                    }
                }
            }
        }
    }


    public void ShowCurrentNums()
    {
        var num = (IndexesAI.Length + IndexesReg.Length);
        joined.text = num.ToString();
        remaining.text = (5-num).ToString();
        joined.gameObject.SetActive(true);
        remaining.gameObject.SetActive(true);

    }

    PlayerListState CheckState()
    {
        if (lobbyData.joinedLobby == null) return PlayerListState.EditTypes;
        else if (IndexesReg != null) return PlayerListState.PlayersJoining;
        else return PlayerListState.CreateLobby;
    }

    public void ShowCurrentPlayers()
    {
        var state = CheckState();
        switch (state)
        {
            case PlayerListState.EditTypes:
                RefreshAIPlayers();
                break;
            case PlayerListState.CreateLobby:
                break;
            case PlayerListState.PlayersJoining:
                if (!lobbyData.ImLobbyHost())
                {
                    RefreshAIPlayers();
                    DisableTypeButtons();
                }
                RefreshRegPlayers();
                break;
        }


        ShowCurrentNums();    
    }
}
