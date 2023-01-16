using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : NetworkBehaviour, IDataPersistence
{
    public int[] cardsQuantityPerColor; // liczba kart dla każdego koloru
    public Sprite[] sprites; // kolory kart
    // nazwy zbiorów kart
    public string[] names = {"RedCards","GreenCards","BlueCards","YellowCards","PinkCards","RainbowCards"};
    int[] startHand = new int[] {1,1,1,1,1,1};
    private GameManager gameManager;

    //private int[][] cardsQuantityPerPlayerPerColor;
    public Dictionary<int, int[]> cardsQuantityPerPlayerPerColor = new();

    void Awake()
    {
        cardsQuantityPerPlayerPerColor = new();//new int[Server.allPlayersInfo.Count][];

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int colorsNumber = cardsQuantityPerColor.Length;

        for (int i = 0; i < colorsNumber; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            //card.transform.GetChild(0).GetComponent<TMP_Text>().text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            
            var counter = card.transform.GetChild(0).GetComponent<TMP_Text>();
            counter.text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            gameManager.cardStackCounterList.Add(counter);

            int ii = i;
            card.GetComponent<Button>().onClick.AddListener(() => Communication.ChooseCard((Color)ii));
        }

        Destroy(cardTempalte);
        Debug.Log("Ładuję card deck");
        SendCardsStacksServerRpc(startHand,PlayerGameData.Id);
    }

    public void LoadData(GameData data)
    {
        if (IsHost)
        {
            for (int i = 0; i < gameManager.cardStackCounterList.Count; i++)
            {
                Debug.Log($"Color: {(Color)i}");
                gameManager.cardStackCounterList[i].text = data.cardsForEachPalyer[PlayerGameData.Id][i].ToString();
                PlayerGameData.numOfCardsInColor[(Color)i] = data.cardsForEachPalyer[PlayerGameData.Id][i];
                Debug.Log($"Cards in this color: {PlayerGameData.numOfCardsInColor[(Color)i]}");
            }
            if (!cardsQuantityPerPlayerPerColor.ContainsKey(PlayerGameData.Id))
               cardsQuantityPerPlayerPerColor.Add(PlayerGameData.Id, data.cardsForEachPalyer[PlayerGameData.Id]);
            else
               cardsQuantityPerPlayerPerColor[PlayerGameData.Id] = data.cardsForEachPalyer[PlayerGameData.Id];

            for (int i = 1; i < Server.allPlayersInfo.Count; i++)
            {
                if (!cardsQuantityPerPlayerPerColor.ContainsKey(Server.allPlayersInfo[i].Id))
                   cardsQuantityPerPlayerPerColor.Add(Server.allPlayersInfo[i].Id, data.cardsForEachPalyer[Server.allPlayersInfo[i].Id]);
                else
                   cardsQuantityPerPlayerPerColor[Server.allPlayersInfo[i].Id] = data.cardsForEachPalyer[Server.allPlayersInfo[i].Id];
                if (!Server.allPlayersInfo[i].IsAI)
                {
                    // ustawiam rpc na wysyłanie do konkretnego gracza (kazdy gracz musi otrzymac inne dane)
                    /*
                    ClientRpcParams clientRpcParams = new()
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { (ulong)Server.allPlayersInfo[i].Id }
                        }
                    };
                    */
                    LoadCardsStacksClientRpc(data.cardsForEachPalyer[Server.allPlayersInfo[i].Id], Server.allPlayersInfo[i].UnityId, Server.allPlayersInfo[i].Id);//, clientRpcParams);
                }
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (IsHost)
        {
            for(int i=0;i<Server.allPlayersInfo.Count;i++)
            {
                if (!cardsQuantityPerPlayerPerColor.ContainsKey(Server.allPlayersInfo[i].Id))
                            cardsQuantityPerPlayerPerColor.Add(Server.allPlayersInfo[i].Id, startHand);

                if (!Server.allPlayersInfo[i].IsAI)
                {
                    if (!data.cardsForEachPalyer.ContainsKey(Server.allPlayersInfo[i].Id))
                        data.cardsForEachPalyer.Add(Server.allPlayersInfo[i].Id, cardsQuantityPerPlayerPerColor[Server.allPlayersInfo[i].Id]);
                    else
                        data.cardsForEachPalyer[Server.allPlayersInfo[i].Id] = cardsQuantityPerPlayerPerColor[Server.allPlayersInfo[i].Id];
                }
                else
                {
                    ArtificialPlayer AI = Server.artificialPlayers.Single(player => Server.allPlayersInfo[i].Id == player.Id);
                    int[] pom = new int[6];
                    for(int j=0;j<6;j++)
                    {
                        pom[j] = AI.numOfCardsInColor[(Color)j];
                    }
                    if (!data.cardsForEachPalyer.ContainsKey(Server.allPlayersInfo[i].Id))
                        data.cardsForEachPalyer.Add(Server.allPlayersInfo[i].Id, pom);
                    else
                        data.cardsForEachPalyer[Server.allPlayersInfo[i].Id] = pom;
                }
            }  
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendCardsStacksServerRpc(int[] cards,int id) //ServerRpcParams serverRpcParams = default)//int redCards, int greenCards, int blueCards, int yellowCards, int pinkCards, int rainbowCards, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Początkowa ręka");
        //int id = (int)serverRpcParams.Receive.SenderClientId;
        if(!cardsQuantityPerPlayerPerColor.ContainsKey(id))
            cardsQuantityPerPlayerPerColor.Add(id, cards);
        else
            cardsQuantityPerPlayerPerColor[id] = cards;
    }

    [ClientRpc]
    void LoadCardsStacksClientRpc(int[] cardStack,string UnityId,int id ,ClientRpcParams clientRpcParams = default)
    {
        if (PlayerGameData.UnityId == UnityId)
        {
            PlayerGameData.Id = id;
            for (int i = 0; i < cardStack.Length; i++)
            {
                Debug.Log($"Color: {(Color)i}");
                gameManager.cardStackCounterList[i].text = cardStack[i].ToString();
                PlayerGameData.numOfCardsInColor[(Color)i] = cardStack[i];
                Debug.Log($"Cards in this color: {PlayerGameData.numOfCardsInColor[(Color)i]}");

            }
        }
    }
}
