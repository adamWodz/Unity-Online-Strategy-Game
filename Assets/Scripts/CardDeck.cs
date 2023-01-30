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
    public int[] cardsQuantityPerColor;      
    public Sprite[] sprites;   
    public string[] names = {"RedCards","GreenCards","BlueCards","YellowCards","PinkCards","RainbowCards"};
    int[] startHand = new int[] {1,1,1,1,1,1};
    private GameManager gameManager;

    public Dictionary<int, int[]> cardsQuantityPerPlayerPerColor = new();

    void Awake()
    {
        cardsQuantityPerPlayerPerColor = new(); 

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int colorsNumber = cardsQuantityPerColor.Length;

        for (int i = 0; i < colorsNumber; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            card.name = names[i];
            
            var counter = card.transform.GetChild(0).GetComponent<TMP_Text>();
            counter.text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            gameManager.cardStackCounterList.Add(counter);

            int ii = i;
            card.GetComponent<Button>().onClick.AddListener(() => Communication.ChooseCard((Color)ii));
        }

        Destroy(cardTempalte);
        if (!Communication.loadOnStart)
        {
            SendCardsStacksServerRpc(startHand, PlayerGameData.Id);
        }
    }

    public void LoadData(GameData data)
    {
        if (IsHost)
        {
            for (int i = 0; i < gameManager.cardStackCounterList.Count; i++)
            {
                gameManager.cardStackCounterList[i].text = data.cardsForEachPalyer[PlayerGameData.Id][i].ToString();
                PlayerGameData.numOfCardsInColor[(Color)i] = data.cardsForEachPalyer[PlayerGameData.Id][i];
            }
            if (!cardsQuantityPerPlayerPerColor.ContainsKey(PlayerGameData.Id))
               cardsQuantityPerPlayerPerColor.Add(PlayerGameData.Id, data.cardsForEachPalyer[PlayerGameData.Id]);
            else
               cardsQuantityPerPlayerPerColor[PlayerGameData.Id] = data.cardsForEachPalyer[PlayerGameData.Id];

            for (int i = 0; i < data.players.Count; i++)
            {
                if (!cardsQuantityPerPlayerPerColor.ContainsKey(data.players[i].Id))
                   cardsQuantityPerPlayerPerColor.Add(data.players[i].Id, data.cardsForEachPalyer[data.players[i].Id]);
                else
                   cardsQuantityPerPlayerPerColor[data.players[i].Id] = data.cardsForEachPalyer[data.players[i].Id];
                    LoadCardsStacksClientRpc(data.cardsForEachPalyer[data.players[i].Id], data.players[i].UnityId, data.players[i].Id); 
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
    public void SendCardsStacksServerRpc(int[] cards,int id)                   
    {
        if(!cardsQuantityPerPlayerPerColor.ContainsKey(id))
            cardsQuantityPerPlayerPerColor.Add(id, cards);
        else
            cardsQuantityPerPlayerPerColor[id] = cards;
    }

    [ClientRpc]
    void LoadCardsStacksClientRpc(int[] cardStack,string UnityId,int id)
    {
        if (PlayerGameData.UnityId == UnityId)
        {
            PlayerGameData.Id = id;
            for (int i = 0; i < cardStack.Length; i++)
            {
                gameManager.cardStackCounterList[i].text = cardStack[i].ToString();
                PlayerGameData.numOfCardsInColor[(Color)i] = cardStack[i];
            }
        }
    }
}
