using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;

public class LobbySetup : MonoBehaviour
{
    private Lobby hostLobby, joinedLobby;
    private float heartbeatTimer, lobbyUpdateTimer;
    private string playerName;

    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () => { Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}"); };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // tutaj mozna prztestowac ponizsze metody
        playerName = $"SpaceExplorer{UnityEngine.Random.Range(10, 99)}";
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimer = 1.1f;
                heartbeatTimer = lobbyUpdateTimer;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    private async void CreateLobby()
    {
        try {
            string lobbyName = "MyLobby";
            int maxPlayers = 5;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions 
            { 
                IsPrivate = true,  // do JoinByCode IsPrivate = true pozosta³e false
                                   // nie bedzie sie pokazywac
                                   // w liscie, trzeba wpisac kod
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    //{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "GameMode1", DataObject.IndexOptions.S1) },
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public, "de_dust2") }
                } // dane wspólne dla jednego lobby
            }; 
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log($"Created Lobby! {lobby.Name} {lobby.MaxPlayers} {lobby.Id} {lobby.LobbyCode}");
            PrintPlayers(hostLobby);
        }
        catch(LobbyServiceException e) { Debug.Log(e); };
    }

    private async void ListLobbies()
    {
        try {

            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder> { new QueryOrder(false, QueryOrder.FieldOptions.Created) }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log($"Lobbies found: {queryResponse.Results.Count}");
            //foreach (Lobby lobby in queryResponse.Results) Debug.Log($"lobby.Name {lobby.MaxPlayers} {lobby.Data["GameMode"].Value}");
            foreach (Lobby lobby in queryResponse.Results) Debug.Log($"lobby.Name {lobby.MaxPlayers}");
        }
        catch(LobbyServiceException e) { Debug.Log(e); }
    }

    private async void JoinLobby()
    {
        try
        {
            // do³¹cza siê do pierwszego lobby
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        } 
        catch(LobbyServiceException e) { Debug.Log(e); }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log($"Joined Lobby with code {lobbyCode}");
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        } catch (LobbyServiceException e) { Debug.Log(e); }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
        };
    }
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }
    private void PrintPlayers(Lobby lobby)
    {
        //Debug.Log($"Players in Lobby {lobby.Name} {lobby.Data["GameMode"].Value} {lobby.Data["Map"].Value}");
        Debug.Log($"Players in Lobby {lobby.Name} {lobby.Data["Map"].Value}");
        foreach (Player player in lobby.Players) { Debug.Log($"player.Id {player.Data["PlayerName"].Value}"); }
    }

    //private async void UpdateLobbyGameMode (string gameMode)
    //{
    //    try
    //    {
    //        hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
    //        {
    //            Data = new Dictionary<string, DataObject>{
    //                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
    //            }
    //        });
    //        PrintPlayers(hostLobby);
    //    }
    //    catch (LobbyServiceException e) { Debug.Log(e); }
    //}

    private async void UpdatePlayerName(string newPlayerName)
    {
        try { 
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                }
            });
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    public async void RemovePlayer(int number)
    {
        try
        {
            if (number<=hostLobby.MaxPlayers && number>0)
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[number-1].Id);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    private async void SwitchLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            { HostId = joinedLobby.Players[1].Id });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    private async void DeleteLobbyManually()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }
}
 