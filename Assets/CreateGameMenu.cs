using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using TMPro;

public class CreateGameMenu : MonoBehaviour
{
    public static CreateGameMenu Instance { get; private set; }

    public Button copyCodeButton;
    public Text codeText;
    public Transform players;
    public Transform playerTemp;

    public Lobby myLobby;
    public int playerCount;
    public TextMeshProUGUI joined, remaining;

    private void Awake()
    {
        Instance = this;
    }

        // Start is called before the first frame update
    void Start()
    {
        LobbySetup.Instance.CreateLobby(UserLogin.playerName + "LOBBY", 5, false, LobbySetup.GameMode.Standard);

        //Application.logMessageReceivedThreaded += HandleLog;
        //LobbySetup.Instance.CreateLobby(UserLogin.playerName + "LOBBY", 5, true, LobbySetup.GameMode.Standard);
        
        //myLobby = LobbySetup.Instance.GetJoinedLobby();

        LobbySetup.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbySetup.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
    }

    private void UpdateLobby_Event(object sender, LobbySetup.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    public int i = 0;
    // Update is called once per frame
    void UpdateLobby()
    {
        ClearLobby();

        foreach (Player player in LobbySetup.Instance.GetJoinedLobby().Players)
        {
            Transform playerSingleTransform = Instantiate(playerTemp, players);
            playerSingleTransform.gameObject.SetActive(true);
            PlayerTemplate PlayerTemplate = playerSingleTransform.GetComponent<PlayerTemplate>();


            PlayerTemplate.UpdatePlayer(player);
        }

        var pNum = LobbySetup.Instance.GetJoinedLobby().Players.Count;
        joined.text = pNum.ToString();
        remaining.text = (5-pNum).ToString();

        Show();
    }

    private void ClearLobby()
    {
        foreach (Transform child in players)
        {
            if (child == playerTemp) continue;
            Destroy(child.gameObject);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    /*
    private void Update()
    {
        if (codeText == null) codeText.text = LobbySetup.Instance.GetLobbyCode();

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        codeText.text = logString;
    }

    public void CodeClick()
    {
        GUIUtility.systemCopyBuffer = codeText.text;
    } 
    */
}
