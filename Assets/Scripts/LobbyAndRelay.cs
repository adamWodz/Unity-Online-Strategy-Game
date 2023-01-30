using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyAndRelay : MonoBehaviour
{
    public MainMenu mainMenu;
    public GameObject createGameMenu, joinMenu, createLobby, protip, createGameBack, quitButton;

    public Lobby joinedLobby;
    public int maxPlayers;

    public Lobby hostLobby;
    private float heartbeatTimer;

    public Text code, onjoin;
    public InputField input;

    public ChooseMapMenu mapMenu;

    public string AISeats;
    public bool showStartGame = false;

    public Button quit, cancel;
    public void PrintClicked()
    {
    }

    private async void Start()
    {

        maxPlayers = 5;
        var options = new InitializationOptions();
#if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif
        await UnityServices.InitializeAsync(options);

        AuthenticationService.Instance.SignedIn += () =>
        {
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    private void Update()
    {
        HandleHeartbeat();
        HandlePolling();
    }
    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatMax = 15;
                heartbeatTimer = heartbeatMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData
                );
            NetworkManager.Singleton.StartHost();
            return joincode;
        }
        catch (RelayServiceException e)
        {
            return null;
        }
    }
    public async void JoinRelay(string joincode)
    {
        try
        {
            JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData,
                a.HostConnectionData
                );
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
        }
    }

    public bool CheckSavedPlayer(string signedin)
    {
        string pstring = PlayerPrefs.GetString("players");
        List<string> playerIDs = new List<string>(pstring.Split(';'));
        return playerIDs.Contains(signedin);
        
    }

    public async void CreatePrivateLobby()
    {
        try
        {
            maxPlayers = 5;       
            string relaycode = await CreateRelay(maxPlayers);
            string username = Assets.GameplayControl.PlayerGameData.Name;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("mojeLobby", maxPlayers,
                new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject> {
                        {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member,relaycode)},
                        {"IndexesAI", new DataObject(DataObject.VisibilityOptions.Public,AISeats)}
                    },
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {"UserName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,username)}
                        }
                    }
                }
            );

            hostLobby = lobby;
            joinedLobby = hostLobby;
            string relaycode2 = lobby.Data["RelayCode"].Value;

            PrintLobbyInfo(lobby);
            if (code.text != null) GUIUtility.systemCopyBuffer = code.text;

            CanStartGame();
        }
        catch (LobbyServiceException e)
        {
        }
    }

    public void CanStartGame()
    {
        if (!showStartGame)
        {
            if (ImLobbyHost() && joinedLobby.Players.Count>1 || joinedLobby.Data["IndexesAI"].Value.Length > 0)
            {
                var foundButton = GameObject.Find("StartGameButton");
                if (foundButton != null)
                {
                    Button startGameButton = foundButton.GetComponent<Button>();
                    startGameButton.interactable = true;
                }
                else        
                showStartGame = true;
            }
        }
    }

    public async void JoinByCode()
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            if (input.text != lobby.LobbyCode)
            {
                joinedLobby = lobby;
                WrongCodeHandle();
            }
            else if (!SeatsLeft(lobby))
            {
                joinedLobby = lobby;
                TooManyPlayersInList();
            }
            else if (Communication.loadOnStart && !CheckSavedPlayer(AuthenticationService.Instance.PlayerId))
            {
                joinedLobby = lobby;
                WrongPlayerHandle();
            }
            else
            {
                if (Communication.loadOnStart)  

                PrintLobbyInfo(lobby);
                PrintPlayers(lobby);
                var playerId = AuthenticationService.Instance.PlayerId;
                string username = Assets.GameplayControl.PlayerGameData.Name;
                var lobby2 = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, playerId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                {
                    {"UserName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,username)}
                }
                });

                lobby = lobby2;

                joinedLobby = lobby;

                if (!ImInLobby())
                {
                    lobby = joinedLobby;
                }

                string relaycode = joinedLobby.Data["RelayCode"].Value;
                if (relaycode != "0")
                {
                    if (!ImLobbyHost())
                    {
                        JoinRelay(relaycode);
                    }
                }
                else      

                RpcRefreshPlayerList();
                CanStartGame();
                RedirectToCreateGameMenu();
            }
        }
        catch (LobbyServiceException e)    
        {
        }
        
    }

    [ClientRpc]
    public void RpcRefreshPlayerList()
    {
        var found = GameObject.Find("PlayerList");
        if (found != null)
        {
            var playerList = found.GetComponent<PlayerList>();

            if (playerList != null)
            {
            }
            else      

            playerList.RefreshList();
        }
        else Debug.Log("[RpcRefreshPlayerList] Cant find playerlist");
    }

    public void RefreshPlayerList()
    {
        if (joinedLobby != null)
        {
            PrintPlayers(joinedLobby);
            if (!ImInLobby())
            {
                RpcRefreshPlayerList();
            }
        }
        if (ImLobbyHost())
        {
            CanStartGame();
            RpcRefreshPlayerList();
        }
    }

    public void PrintLobbyInfo(Lobby lobby)
    {
        code.text = lobby.LobbyCode;
    }
    public void PrintPlayers(Lobby chosen)
    {
        int j = 0, n = chosen.Players.Count;
        foreach (var p in chosen.Players)
        {
            if (p.Data != null) Debug.Log($"[PrintPlayers] Player {j++}/{n} {p.Data != null} {p.Data["UserName"].Value}");
            else Debug.Log($"[PrintPlayers] Player {j++}/{n} {p.Id}");
        }
    }

    private float updateTimer;
    public async void HandlePolling()
    {
        if (joinedLobby != null)
        {
            updateTimer -= Time.deltaTime;
            if (updateTimer < 0f)
            {
                float updateMax = 1.1f;
                updateTimer = updateMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                RefreshPlayerList();
                if(!ImLobbyHost()) RpcRefreshPlayerList();
            }
        }
    }

    public async void WrongPlayerHandle()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            else
            {
            }

            joinMenu.SetActive(true);
            onjoin.text = "Nie uczestniczy³eœ we wczytanej grze!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
        }
    }

    public async void TooManyPlayersInList()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            else
            {
            }

            joinMenu.SetActive(true);
            onjoin.text = "Nie ma teraz wolnego miejsca w lobby!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
        }
    }

    public bool SeatsLeft(Lobby lobby)
    {
        if (lobby.Data == null) Debug.Log(" cant see lobby data");
        else if (!lobby.Data.ContainsKey("IndexesAI")) Debug.Log(" cant see lobby data key");
        else
        {
            var playersai = lobby.Data["IndexesAI"].Value.Length;
            var playersreg = lobby.Players.Count;
            var playersmax = lobby.MaxPlayers;

            return (playersai + playersreg <= playersmax);
        }
        return false;
    }

    public async void WrongCodeHandle()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            else
            {
            }

            joinMenu.SetActive(true);
            onjoin.text = "Wpisa³eœ z³e has³o!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
        }
    }

    public void RedirectToCreateGameMenu()
    {
        createGameMenu.SetActive(true);
        joinMenu.SetActive(false);
        createLobby.SetActive(false);
        protip.SetActive(false);
        createGameBack.SetActive(false);
        quitButton.SetActive(true);
        quitButton.GetComponent<Button>().interactable = true;
    }

    public async void LeaveLobby()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            if (joinedLobby != null && ImInLobby())
            {
                if (!ImLobbyHost())
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                    NetworkManager.Singleton.Shutdown(true);
                }
                else
                {
                    int i = 0;
                    var clientIDs = NetworkManager.Singleton.ConnectedClientsIds;
                    foreach(var p in joinedLobby.Players)
                    {
                        if (clientIDs==null || clientIDs.Count == 0 || i == clientIDs.Count) break;
                        if (p.Id!=joinedLobby.HostId && i < clientIDs.Count)
                        {
                            var clientId = clientIDs[i++];
                            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, p.Id);
                            NetworkManager.Singleton.DisconnectClient(clientId);
                        }
                    }
                    NetworkManager.Singleton.Shutdown(true);
                    AuthenticationService.Instance.SignOut();
                    NetworkManager.Singleton.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
            }
            else
            {
            }

        }
        catch (LobbyServiceException e)
        {
        }
    }

    public void KickRecentlyJoined()
    {
        var n = joinedLobby.Players.Count;
        var recent = joinedLobby.Players[n-1].Id;
        KickPlayer(recent);
    }

    public async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
        }
    }
    public async void CloseLobby()
    {
        try
        {
            foreach (var player in joinedLobby.Players) if(joinedLobby.HostId!=player.Id) KickPlayer(player.Id);
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
        }
    }

    public NetworkClient findClient(int clientID)
    {
        var clients = NetworkManager.Singleton.ConnectedClients;
        NetworkClient found;
        clients.TryGetValue((ulong)clientID, out found);
        return found;
    }

    public void printClients()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds;
        int i = 1;
        foreach (var c in clients) Debug.Log($"[printClients] {i++}.clientID {c}");
    }



    public bool ImLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public bool IsInLobby(string playerId, Lobby chosen)
    {
        if (chosen != null && chosen.Players != null)
        {
            foreach (Player player in chosen.Players)
            {
                if (player.Id == playerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool ImInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

}
