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

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyAndRelay : MonoBehaviour
{
    public Lobby joinedLobby;
    public int maxPlayers;

    public Lobby hostLobby;
    private float heartbeatTimer;

    public Text code, onjoin;
    public InputField input;

    public ChooseMapMenu mapMenu;

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
        catch (RelayServiceException e) {
            Debug.Log($"[JoinRelay]! {e}"); }
    }

    // Lobby

    public async void CreatePrivateLobby()
    {
        try
        {
            maxPlayers = 5; // remove after creating the player list
            //1. by code (private) + Relay//

            string relaycode = await CreateRelay(maxPlayers);
            Debug.Log($"[CreatePrivateLobby] 1/2 Created Relay with {maxPlayers} connections");

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("mojeLobby", maxPlayers,
                new CreateLobbyOptions
                {
                    IsPrivate = true,
                    Data = new Dictionary<string, DataObject>
                { {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member,relaycode)} }
                }
            );
            
            //2,3. public//
            //Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("mojeLobby", maxPlayers);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            string relaycode2 = lobby.Data["RelayCode"].Value;

            Debug.Log($"[CreatePrivateLobby] 2/2 Created Lobby {lobby.Name} ({lobby.Id}) with data {relaycode2}={relaycode}?");
            PrintLobbyInfo(lobby);
            if (code.text != null) GUIUtility.systemCopyBuffer = code.text;

            //QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();
            //Debug.Log(lobbies.Results.Count);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[CreatePrivateLobby]! { e}");
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
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(input.text);
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

            joinedLobby = lobby;

            if (!IsPlayerInLobby())
            {
                lobby = joinedLobby;
                //joinedLobby = null;//
            }

            string relaycode = joinedLobby.Data["RelayCode"].Value;
            Debug.Log($"[JoinByCode] 2/4 Relay code: {joinedLobby != null} {joinedLobby.Data != null} {relaycode}");

            if (relaycode != "0")
            {
                if (!IsLobbyHost())
                {
                    JoinRelay(relaycode);
                    Debug.Log($"[JoinByCode] 3/4 Joined a Relay");
                    //mapMenu.StartGame();//
                    Debug.Log($"[JoinByCode] 4/4 Joined Game");
                }
                //joinedLobby = null;//
            }
            else Debug.Log($"[JoinByCode]! NOT CONNECTED to a Relay");

        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[JoinByCode]! {e}");
            onjoin.text = e.Message.ToString();
        }
    }

    public void PrintLobbyInfo(Lobby lobby)
    {
        code.text = lobby.LobbyCode;
        onjoin.text = lobby.Name + " " + lobby.Id.Substring(0, 5);
    }
    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"[PrintPlayers] In lobby {lobby.Id}");
        foreach (var p in lobby.Players) Debug.Log("[PrintPlayers] Player " + p.Id);
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
            }
        }
    }


    public async void LeaveLobby()
    {
        try
        {
            Debug.Log($"[LeaveLobby] 1/2 As A Host? {IsLobbyHost()}");
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            Debug.Log($"[LeaveLobby] 2/2 I'm Leaving Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[LeaveLobby]! {e}");
        }
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
            Debug.Log($"[CloseLobby] 1/2 Closing Lobby existing ({joinedLobby!=null}) {joinedLobby.Id}");
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            Debug.Log($"[CloseLobby] 2/2 Closed my Lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"[CloseLobby] {e}");
        }
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
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
