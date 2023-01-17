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

    // do zapisu przy utworzeniu lobby które seats z PlayerList s¹ AI
    public string AISeats;
    public bool showStartGame = false;

    public Button quit, cancel;
    public void PrintClicked()
    {
        Debug.Log($"Cancel{cancel.IsActive()} Confirm{quit.IsActive()}");
    }

    // Start is called before the first frame update
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
            Debug.Log($"Signed in as {AuthenticationService.Instance.PlayerId}");
        };
        //AuthenticationService.Instance.ClearSessionToken();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log($"{(int)NetworkManager.Singleton.LocalClientId}");
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

    // Relay

    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joincode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
            Debug.Log($"[CreateRelay] 1/3 Relay code: {joincode}");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData
                );
            Debug.Log($"[CreateRelay] 2/3 Set Relay data.");
            NetworkManager.Singleton.StartHost();
            Debug.Log($"[CreateRelay] 3/3 Started as a HOST");
            return joincode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log($"[CreateRelay]! {e}");
            return null;
        }
    }
    public async void JoinRelay(string joincode)
    {
        try
        {
            Debug.Log($"[JoinRelay] 1/3 Joined Relay with code: {joincode}");
            JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joincode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                a.RelayServer.IpV4,
                (ushort)a.RelayServer.Port,
                a.AllocationIdBytes,
                a.Key,
                a.ConnectionData,
                a.HostConnectionData
                );
            Debug.Log($"[JoinRelay] 2/3 Set client data");
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[JoinRelay] 3/3 Started as a CLIENT");
        }
        catch (RelayServiceException e)
        {
            Debug.Log($"[JoinRelay]! {e}");
        }
    }

    // Lobby

    public bool CheckSavedPlayer(string signedin)
    {
        //var signedin = AuthenticationService.Instance.PlayerId;
        string pstring = PlayerPrefs.GetString("players");
        List<string> playerIDs = new List<string>(pstring.Split(';'));
        return playerIDs.Contains(signedin);
        
    }

    public async void CreatePrivateLobby()
    {
        try
        {
            maxPlayers = 5; // remove after creating the player list
            //1. by code (private) + Relay//

            string relaycode = await CreateRelay(maxPlayers);
            Debug.Log($"[CreatePrivateLobby] 1/3 Created Relay with {maxPlayers} connections");
            string username = Assets.GameplayControl.PlayerGameData.Name;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("mojeLobby", maxPlayers,
                new CreateLobbyOptions
                {
                    //IsPrivate = true,
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject> {
                        {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member,relaycode)},
                        {"IndexesAI", new DataObject(DataObject.VisibilityOptions.Public,AISeats)}
                    },
                    // ustawienie nazwy hosta
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {"UserName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,username)}
                        }
                    }
                }
            );

            //2,3. public//
            //Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("mojeLobby", maxPlayers);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            string relaycode2 = lobby.Data["RelayCode"].Value;

            Debug.Log($"[CreatePrivateLobby] 2/3 Created Lobby {lobby.Name} ({lobby.Id}) with data {relaycode2}={relaycode}?");
            PrintLobbyInfo(lobby);
            if (code.text != null) GUIUtility.systemCopyBuffer = code.text;

            Debug.Log($"[CreatePrivateLobby] 3/3 PLAYER = {joinedLobby.Players[0].Data["UserName"].Value}");
            CanStartGame();
            //QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            //Debug.Log(lobbies.Results.Count);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[CreatePrivateLobby]! {e}");
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
                else Debug.Log("[CanStartGame] Can't find StartButton, but can start game");
                showStartGame = true;
            }
        }
    }

    public async void JoinByCode()
    {
        try
        {
            //QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            //int lobbiesnum = lobbies.Results.Count;
            //Debug.Log(lobbiesnum);
            //onjoin.text = $"There's {lobbiesnum} lobbies";

            //1. by code -//
            //Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(input.text);
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            if (input.text != lobby.LobbyCode)
            {
                Debug.Log("WRONG CODE");
                joinedLobby = lobby;
                WrongCodeHandle();
            }
            else if (!SeatsLeft(lobby))
            {
                Debug.Log("TOO MANY PLAYERS");
                joinedLobby = lobby;
                TooManyPlayersInList();
            }
            else if (Communication.loadOnStart && !CheckSavedPlayer(AuthenticationService.Instance.PlayerId))
            {
                Debug.Log("PLAYER NOT SAVED");
                joinedLobby = lobby;
                WrongPlayerHandle();
            }
            else
            {
                if (Communication.loadOnStart) Debug.Log("LOADING GAME");

                PrintLobbyInfo(lobby);
                PrintPlayers(lobby);
                Debug.Log($"[JoinByCode] 1/4 Joined to a Lobby {lobby.Id}");

                //2. first created lobby +//
                //QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
                //Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(response.Results[0].Id);
                //onjoin.text = $"Lobby created {response.Results[0].Created}";

                //3. quickjoin +//
                //Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                //onjoin.text = $"Joined with quickjoin";

                var playerId = AuthenticationService.Instance.PlayerId;
                string username = Assets.GameplayControl.PlayerGameData.Name;
                var lobby2 = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, playerId, new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                {
                    {"UserName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,username)}
                }
                });

                Debug.Log($"[JoinByCode] 1.2/4 UpdatePlayer {lobby.Id} {lobby2.Id}");
                lobby = lobby2;

                joinedLobby = lobby;

                if (!ImInLobby())
                {
                    lobby = joinedLobby;
                    //joinedLobby = null;//
                }

                string relaycode = joinedLobby.Data["RelayCode"].Value;
                Debug.Log($"[JoinByCode] 2/4 Relay code: {joinedLobby != null} {joinedLobby.Data != null} {relaycode}");

                if (relaycode != "0")
                {
                    if (!ImLobbyHost())
                    {
                        JoinRelay(relaycode);
                        Debug.Log($"[JoinByCode] 3/4 Joined a Relay");
                        //mapMenu.StartGame();//
                        Debug.Log($"[JoinByCode] 4/4 Joined Game");
                    }
                    //joinedLobby = null;//
                }
                else Debug.Log($"[JoinByCode]! NOT CONNECTED to a Relay");

                RpcRefreshPlayerList();
                CanStartGame();
                RedirectToCreateGameMenu();
            }
        }
        catch (LobbyServiceException e) //when (e.ErrorCode == 16001)
        {
            Debug.Log(e);
            /**
            Debug.Log($"WrongCode! {e.ErrorCode == 16001 && e.Message.Contains("err fetching lobby id by code: ResultCode: KEY_NOT_FOUND_ERROR")}");
            try { Debug.Log("Catch other error"); }
            catch (LobbyServiceException e2) //when (e2.ErrorCode == 16001)
            {
                Debug.Log($"{e2.ErrorCode}");
                //GameObject.Find("")
                NetworkManager.Singleton.Shutdown(true);
                try
                {
                    Debug.Log($"{e2.ErrorCode}");
                }
                catch (LobbyServiceException e3) //when (e3.ErrorCode == 16000)
                {
                    Debug.Log($"{e3.ErrorCode}");
                    //GameObject.Find("")
                    try
                    {
                        Debug.Log($"{e3.ErrorCode}");
                    }
                    catch (LobbyServiceException e4) //when (e4.ErrorCode == 16000)
                    {
                        Debug.Log($"{e4.ErrorCode}");
                        //GameObject.Find("")
                        NetworkManager.Singleton.Shutdown(true);
                    }
                }
            }
            */
        }
        
    }

    [ClientRpc]
    public void RpcRefreshPlayerList()
    {
        Debug.Log($"##{ImInLobby()} {AuthenticationService.Instance.PlayerId}");
        var found = GameObject.Find("PlayerList");
        if (found != null)
        {
            var playerList = found.GetComponent<PlayerList>();

            Debug.Log($"[RpcRefreshPlayerList] 1/2 HOST:{ImLobbyHost()}");
            if (playerList != null)
            {
                Debug.Log($"[RpcRefreshPlayerList] 2/2 {playerList.seats.Count} seats where {AISeats} are AI");
            }
            else Debug.Log($"[RpcRefreshPlayerList] 2/2 PlayerList gameObject is null");

            playerList.RefreshList();
        }
        else Debug.Log("[RpcRefreshPlayerList] Cant find playerlist");
    }

    public void RefreshPlayerList()
    {
        if (joinedLobby != null)
        {
            Debug.Log("[RefreshPlayerList] CHECK PLAYERS IN LOBBY");
            PrintPlayers(joinedLobby);
            if (!ImInLobby())
            {
                RpcRefreshPlayerList();
                //if (!ImLobbyHost()) LeaveLobby();
            }
            //else if (ImLobbyHost()) showStartGame = joinedLobby.Players.Count > 1;
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
        Debug.Log($"[PrintPlayers] In lobby {chosen.Id} {ImInLobby()}");
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
            Debug.Log($"[WrongPlayerHandle] 1/2 Leaving {joinedLobby != null && ImInLobby()}");
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                Debug.Log($"After removing {playerId}, he is in lobby? {ImInLobby()}");
                Debug.Log($"[WrongPlayerHandle] 2/2 I'm Leaving Lobby");
            }
            else
            {
                Debug.Log($"[WrongPlayerHandle] 2/2 No joined lobby to leave");
            }

            joinMenu.SetActive(true);
            onjoin.text = "Nie uczestniczy³eœ we wczytanej grze!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void TooManyPlayersInList()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"[WrongCodeHandle] 1/2 Leaving {joinedLobby != null && ImInLobby()}");
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                Debug.Log($"After removing {playerId}, he is in lobby? {ImInLobby()}");
                Debug.Log($"[WrongCodeHandle] 2/2 I'm Leaving Lobby");
            }
            else
            {
                Debug.Log($"[WrongCodeHandle] 2/2 No joined lobby to leave");
            }

            joinMenu.SetActive(true);
            onjoin.text = "Nie ma teraz wolnego miejsca w lobby!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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

            Debug.Log($" {playersai}+{playersreg}<={playersmax}");
            return (playersai + playersreg <= playersmax);
        }
        return false;
    }

    public async void WrongCodeHandle()
    {
        try
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"[WrongCodeHandle] 1/2 Leaving {joinedLobby != null && ImInLobby()}");
            if (joinedLobby != null && ImInLobby())
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                Debug.Log($"After removing {playerId}, he is in lobby? {ImInLobby()}");
                Debug.Log($"[WrongCodeHandle] 2/2 I'm Leaving Lobby");
            }
            else
            {
                Debug.Log($"[WrongCodeHandle] 2/2 No joined lobby to leave");
            }

            joinMenu.SetActive(true);
            onjoin.text = "Wpisa³eœ z³e has³o!";
            onjoin.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
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
            Debug.Log($"[LeaveLobby] 1/2 Leaving {joinedLobby!=null && ImInLobby()}");
            if (joinedLobby != null && ImInLobby())
            {
                if (!ImLobbyHost())
                {
                    await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
                    Debug.Log($"After removing {playerId}, he is in lobby? {ImInLobby()}");
                    NetworkManager.Singleton.Shutdown(true);
                    Debug.Log($"[LeaveLobby] 2/2 I'm Leaving Lobby");
                }
                else
                {
                    Debug.Log("Host leaving with X - deleting all other players");
                    int i = 0;
                    var clientIDs = NetworkManager.Singleton.ConnectedClientsIds;
                    foreach(var p in joinedLobby.Players)
                    {
                        if (clientIDs==null || clientIDs.Count == 0 || i == clientIDs.Count) break;
                        if (p.Id!=joinedLobby.HostId && i < clientIDs.Count)
                        {
                            var clientId = clientIDs[i++];
                            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, p.Id);
                            Debug.Log($"After removing {p.Id}, he is in lobby? {IsInLobby(p.Id, joinedLobby)}");
                            NetworkManager.Singleton.DisconnectClient(clientId);
                            Debug.Log($"[LeaveLobby] 2/2 I'm Leaving Lobby");
                        }
                    }
                    //await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.HostId);
                    NetworkManager.Singleton.Shutdown(true);
                    AuthenticationService.Instance.SignOut();
                    NetworkManager.Singleton.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Additive);
                }
            }
            else
            {
                Debug.Log($"[LeaveLobby] 2/2 No joined lobby to leave");
            }

            //createGameMenu.SetActive(false);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[LeaveLobby]! {e}");
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
            Debug.Log($"[KickPlayer] 1/2 playerId:{playerId != null}");
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            Debug.Log($"[KickPlayer] 2/2 Player {playerId} kicked");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[KickPlayer]! {e}");
        }
    }
    public async void CloseLobby()
    {
        try
        {
            Debug.Log($"[CloseLobby] 1/2 Closing Lobby existing ({joinedLobby != null}) {joinedLobby.Id}");
            foreach (var player in joinedLobby.Players) if(joinedLobby.HostId!=player.Id) KickPlayer(player.Id);
            //KickPlayer(joinedLobby.HostId);
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            Debug.Log($"[CloseLobby] 2/2 Closed my Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[CloseLobby] {e}");
        }
    }

    public NetworkClient findClient(int clientID)
    {
        // jak null to wyrzuci³o klienta, mo¿na jeszcze w grze skorzystaæ z eventu NetworkManager.Singleton.OnClientDisconnect(clientID)? coœ takiego
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
                    // This player is in this lobby
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
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

}
