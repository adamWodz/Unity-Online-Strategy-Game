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
    public List<PlayerSeat> seats;
    public List<bool> RegularPlayers;
    public LobbyAndRelay lobbyData;
    public Text joined, remaining;
    private int joinedNum;

    private void Start()
    {
        RefreshList();
    }

    private void Update()
    {
        RefreshNum();        
    }

    void RefreshNum()
    {
        joinedNum = 0;
        foreach (var p in seats)
        {
            if (p.AI || p.Nickname.text!="") joinedNum++;
        }
        joined.text = joinedNum.ToString();
        remaining.text = (5 - joinedNum).ToString();
    }

    public void RefreshList()
    {
        try
        {
            bool cankick = false;
            if (lobbyData != null && lobbyData.joinedLobby != null)
            {
                if (lobbyData.ImLobbyHost())
                {
                    cankick = true;
                    lobbyData.printClients();
                }
                else
                {
                    for (int j = 0; j < seats.Count; j++) seats[j].AI = !RegularPlayers[j];
                }

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
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void SetPlayerNick(int j, string pref, string playerId)
    {
        seats[j].Nickname.text = $"@ {pref}_{playerId.Substring(0,5)}";
    }
    public void NonAI()
    {
        RegularPlayers = new List<bool>();
        int res = 0;
        foreach (var s in seats)
        {
            if (!s.AI) res++;
            RegularPlayers.Add(!s.AI);
        }
        lobbyData.maxPlayers = res;
        Debug.Log($"[PlayerList.NonAI] MaxPlayer = {res}");
        for (int j = 0; j < seats.Count; j++) seats[j].PlayerType.interactable = false;
        lobbyData.regularPlayers = RegularPlayers;
    }
}
