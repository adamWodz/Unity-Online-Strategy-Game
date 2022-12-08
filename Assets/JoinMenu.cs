using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
public class JoinMenu : MonoBehaviour
{
    public Text Message;
    // Start is called before the first frame update
    private void Start()
    {
        LobbySetup.Instance.QuickJoinLobby();
        Message.text = LobbySetup.Instance.GetJoinedLobby().Id.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
