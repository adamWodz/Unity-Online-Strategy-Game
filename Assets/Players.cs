using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class Players : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform playerSingleTemplate;
    public Transform container;

    public Lobby myLobby;
    public int playerCount;
    public Text joined, remaining;



    void Start()
    {
        myLobby = LobbySetup.Instance.GetJoinedLobby();

        LobbySetup.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbySetup.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        Hide();
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

        foreach (Player player in myLobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            PlayerTemplate PlayerTemplate = playerSingleTransform.GetComponent<PlayerTemplate>();


            PlayerTemplate.UpdatePlayer(player);
        }


        joined.text = myLobby.Players.Count + "/" + myLobby.MaxPlayers;
        
        Show();
    }

    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

}
