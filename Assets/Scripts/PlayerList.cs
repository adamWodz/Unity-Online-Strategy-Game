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
    //public List<bool> RegularPlayers;
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
        //RefreshNum();        
    }

    void RefreshNum()
    {
        /**
        joinedNum = 0;
        foreach (var p in seats)
        {
            if (p.AI || p.Nickname.text!="") joinedNum++;
        }
        joined.text = joinedNum.ToString();
        remaining.text = (5 - joinedNum).ToString();
        */
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
            /**
            bool cankick = false;
            if (lobbyData != null && lobbyData.joinedLobby != null)
            {
                if (lobbyData.ImLobbyHost()) lobbyData.printClients();
                //else if (RegularPlayers.Count == seats.Count)
                //{
                //    Debug.Log($"{seats.Count} {RegularPlayers.Count}");
                //    for (int j = 0; j < seats.Count; j++) seats[j].AI = !RegularPlayers[j];
                //}

                var players = lobbyData.joinedLobby.Players;
                
                int i = 0, a = 0;
                for (int j = 0; j < seats.Count; j++)
                {
                    if (i == players.Count) break;
                    if (!seats[j].AI && seats[j].player == null)
                    {
                        //seats[j].lobby = lobbyData;
                        string pref = (i == 0) ? "host" : "spaceman";
                        var playerId = players[i++].Id;
                        
                        seats[j].playerId = playerId;
                        seats[j].DisplayJoined(cankick);
                        SetPlayerNick(j, pref, playerId);
                    }
                }
            }
            else
            {
                Debug.Log($"[PlayerList.RefreshList] 1/2 Am I the host? {lobbyData.ImLobbyHost()}");
                var j = lobbyData.joinedLobby != null;
                var code = lobbyData.joinedLobby != null ? lobbyData.joinedLobby.Id : "not connected";
                Debug.Log($"[PlayerList.RefreshList] 2/2 joinedLobby code: {code}");
            }
            */

            ShowCurrentPlayers();
            if (lobbyData.joinedLobby != null)
            {
                if (!lobbyData.ImInLobby())
                {
                    Debug.Log("IM NOT IN LOBBY");
                    mainMenu.gameObject.SetActive(true);
                    createGameMenu.SetActive(false);
                }
            }
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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

        /**
        //RegularPlayers = new List<bool>();
        // lobbyData.AISeats = "";
        // foreach (var s in seats)
        // {
        //     if (!s.AI)
        //     {
        //         res++;
        //     }
        //     else
        //     {
        //         lobbyData.AISeats += i.ToString();
        //         Debug.Log(s);
        //     }
        //     //RegularPlayers.Add(!s.AI);
        //     i++;
        // }
        // SetAINicknames(lobbyData.AISeats);
        // lobbyData.maxPlayers = res;
        */

        RefreshAIPlayers();
        lobbyData.AISeats = IndexesAI;
    
        Debug.Log($"[PlayerList.NonAI] MaxPlayer = {res}");
        for (int j = 0; j < seats.Count; j++) seats[j].PlayerType.interactable = false;
    }


    /*
     * seats ma elementy PlayerSeat z pustymi nickami i 
     */
    public enum PlayerListState { EditTypes, CreateLobby, PlayersJoining }
    public string IndexesAI, IndexesReg;
    PlayerListState State;

    public void RefreshAIPlayers()
    {
        if (lobbyData.joinedLobby == null)
        {
            // nadpisywanie
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

        Debug.Log($"[RefreshAIPlayers] {lobbyData.joinedLobby == null} AI: {IndexesAI}");

        // pokazywanie na scenie (powinno by� przy klikni�ciu dowolnego playera)
        int a = 1;
        foreach (char j in IndexesAI)
        {
            var jj = int.Parse(j.ToString());
            if (jj > 0)
            {
                if (!seats[jj].AI) seats[jj].ChangePlayerType();
                //seats[jj].Nickname.text = $"AIplayer{a++}";
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
        // odświeżenie graczy AI
        // zablokowanie przycisk�w (modyfikacja PlayerType PlayerSeat's�w na disabled)
        // przekazanie IndexesAI do lobbyData.AISeats
        /// przy tworzeniu Lobby ustawi si� .Data["IndexesAI"]

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
                else Debug.Log("# no lobby data key");
            }
            else Debug.Log("# no lobby data");

            var foundLobby = GameObject.Find("LobbyAndRelay");
            if (lobby != null && foundLobby != null)
            {
                var lobby2 = foundLobby.GetComponent<LobbyAndRelay>();
                Debug.Log($"Found {lobby2.joinedLobby != null}, attached to the one with {lobby.Players.Count}");
                if (lobby2.joinedLobby != null) Debug.Log($"{lobby.Players.Count}={lobby2.joinedLobby.Players.Count}?");
                players = lobby2.joinedLobby.Players;
            }
            else Debug.Log($"{lobby != null} {foundLobby != null}");

            if (players == null || players.Count > 0)
            {
                int r = 0;
                bool freeSeat, playersLeft;
                for (int i = 0; i < seats.Count; i++)
                {
                    if (players.Count == IndexesReg.Length) break;
                    freeSeat = !indexesAI.Contains(i.ToString()) && !IndexesReg.Contains(i.ToString());
                    Debug.Log($"[RefreshRegPlayers] Seat{i} is free:{freeSeat} IndReg: {IndexesReg}");
                    if (freeSeat) IndexesReg += i.ToString();
                }

                Debug.Log($"[RefreshRegPlayers] RegularPlayers indexes {IndexesReg}, {players.Count} in lobby");
                // pokazywanie na scenie (powinno by� przy do��czaniu dowolnego playera)
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
                            Debug.Log($"[RefreshRegPlayers] {r}th LobbyMember name not saved");
                            if (!lobbyData.ImLobbyHost()) username = Assets.GameplayControl.PlayerGameData.Name;
                        }

                        var jj = int.Parse(j.ToString());
                        if (players[r].Id == Unity.Services.Authentication.AuthenticationService.Instance.PlayerId)
                        {
                            Debug.Log($"& my client found with {username} {Assets.GameplayControl.PlayerGameData.Name}");
                        }

                        seats[jj].playerId = players[r++].Id;
                        Debug.Log($"[RefreshRegPlayers] {seats[jj].playerId}");

                        seats[jj].Nickname.text = username;
                        seats[jj].DisplayJoined(false);
                        //SetPlayerNick(jj, pref, playerId);
                    }
                }

                if (oldIndexesReg.Length > IndexesReg.Length)
                {
                    //czyszczenie starych graczy
                    foreach (char j in oldIndexesReg)
                    {
                        if (!IndexesReg.Contains(j.ToString()))
                        {
                            var jj = int.Parse(j.ToString());
                            seats[jj].FreeSeat();
                            Debug.Log($"[RefreshRegPlayers] {jj} Free");
                        }
                    }
                }
            }
            else Debug.Log($"[RefreshRegPlayers] No Lobby members to display");
        }
    }


    public void ShowCurrentNums()
    {
        var num = (IndexesAI.Length + IndexesReg.Length);
        joined.text = num.ToString();
        remaining.text = (5-num).ToString();
        joined.gameObject.SetActive(true);
        remaining.gameObject.SetActive(true);

        Debug.Log($"[ShowCurrentNums] {IndexesAI.Length} {IndexesReg.Length} || {remaining.IsActive()} {remaining.text}");
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
        Debug.Log($"& {state} {lobbyData!=null}");
        switch (state)
        {
            case PlayerListState.EditTypes:
                // przej�cie przez seats, kt�re s� AI (zapis u nas w stringu IndexesAI)
                // pokazanie ponownej numeracji tych AI (modyfikacja Nickname AI PlayerSeat's�w)
                RefreshAIPlayers();
                break;
            case PlayerListState.CreateLobby:
                /// NonAI
                // zablokowanie przycisk�w (modyfikacja PlayerType PlayerSeat's�w na disabled)
                // przekazanie IndexesAI do lobbyData.AISeats
                // przy tworzeniu Lobby ustawi si� .Data["IndexesAI"]
                // przej�cie r�wnoleg�e przez seats,kt�re nie s� AI oraz .Players (zapis u nas w stringu IndexesReg)
                // pokazanie nowych graczy-hosta (modyfikacja ca�ego PlayerSeat nowo napotkanych)
                //RefreshRegPlayers();
                break;
            case PlayerListState.PlayersJoining:
                // pobranie .Players z lobbyData.joinedLobby ORAZ .Data["IndexesAI"]
                // przej�cie r�wnoleg�e przez seats,kt�re nie s� (*)AI oraz .Players (zapis u nas w stringu (*)IndexesReg)
                // (*)nie ma ich indeksu ani w IndexesAI ani IndexesReg, jak ju� IndexesReg nie jest nullem
                // pokazanie nowych graczy (modyfikacja ca�ego PlayerSeat nowo napotkanych)
                if (!lobbyData.ImLobbyHost())
                {
                    RefreshAIPlayers();
                    DisableTypeButtons();
                }
                RefreshRegPlayers();
                break;
        }


        // edycja
        //RefreshAIPlayers();
        //RefreshRegPlayers();
        ShowCurrentNums();// d�ugo�� IndexesAI+IndexesReg to joined
    }
}
