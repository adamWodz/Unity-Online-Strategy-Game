using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
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

    private GameManager gameManager;

    //private int[][] cardsQuantityPerPlayerPerColor;
    private Dictionary<int,string> cardsQuantityPerPlayerPerColor;

    void Start()
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

        //SendCardsStacksServerRpc(cardsQuantityPerColor);
    }

    public void LoadData(GameData data)
    {
        if (IsHost)
        {
            for (int i = 0; i < gameManager.cardStackCounterList.Count; i++)
            {
                gameManager.cardStackCounterList[i].text = data.cardsForEachPalyer[PlayerGameData.Id][i].ToString();
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (IsHost)
        {
            for(int i=0;i<Server.allPlayersInfo.Count;i++)
            {
                    if (!data.cardsForEachPalyer.ContainsKey(i))
                        data.cardsForEachPalyer.Add(i, cardsQuantityPerPlayerPerColor[i]);
                    else
                        data.cardsForEachPalyer[i] = cardsQuantityPerPlayerPerColor[i];
            }  
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendCardsStacksServerRpc(string cards, ServerRpcParams serverRpcParams = default)//int redCards, int greenCards, int blueCards, int yellowCards, int pinkCards, int rainbowCards, ServerRpcParams serverRpcParams = default)
    {
        int id = (int)serverRpcParams.Receive.SenderClientId;
        if(!cardsQuantityPerPlayerPerColor.ContainsKey(id))
            cardsQuantityPerPlayerPerColor.Add(id, cards);
        else
            cardsQuantityPerPlayerPerColor[id] = cards;
    }
}
