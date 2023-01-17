using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UI;

public class DrawCardsPanel : NetworkBehaviour
{
    public Sprite[] sprites; // kolory kart
    public string[] names = { "RedCard", "GreenCard", "BlueCard", "YellowCard", "PinkCard", "RainbowCard" };
    public List<GameObject> cards = new();
    public GameObject cardPrefab;
    GameManager gameManager;
    Button drawCardsButton;
    public int[] actualCardColor = new int[5];
    CardDeck cardDeck;
    private int numberOfRainbowCards = 0;
    void Start()
    {
        cardDeck = GameObject.Find("CardDeck").GetComponent<CardDeck>();
        
        drawCardsButton = GameObject.Find("DrawCardsButton").GetComponent<Button>();
        
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        int i = 0;
        foreach(Transform child in transform)
        {
            if (child.CompareTag("Cards"))
            {
                int copy = i;
                int color = 3;
                RandomSprite(ref color);
                if ((Color)color == Color.special)
                    numberOfRainbowCards++;
                actualCardColor[copy] = color;
                child.GetComponent<Image>().sprite = sprites[color];
                child.name = names[color];
                child.gameObject.GetComponent<Button>().onClick.AddListener(() => Communication.DrawCard(this, copy));
                i++;
                cards.Add(child.gameObject);
                
                // Pocz¹tkowa synchronizacja kolorów kart
                SyncSpritesServerRpc(color, copy);
            }
        }
        
        while(numberOfRainbowCards >=3)
        {
            ShuffleCards();
        }
        
        drawCardsButton.onClick.AddListener(() => Communication.DrawCard(this,10));
    }
    
    [ClientRpc]
    void SyncSpritesClientRpc(int color, int index)
    {
        ChangeCardColor(color, index);
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncSpritesServerRpc(int color,int index)
    {
        SyncSpritesClientRpc(color, index);
    }

    void ChangeCardColor(int color, int index)
    {
        //Debug.Log($"Index: {index}");
        actualCardColor[index] = color;
        //Debug.Log($"Color: {color}");
        if (cards.Count > index) // klient mo¿e nie utworzyæ listy w tym samym czasie co host
        {
            cards[index].GetComponent<Image>().sprite = sprites[color];
            cards[index].name = names[color];
        }
    }

    Sprite RandomSprite(ref int index)
    {
        index = RandomColorIndex();
        return sprites[index];
    }

    public int RandomColorIndex()
    {
        return UnityEngine.Random.Range(0, sprites.Length); ;
    }

    public Color MoveCard(int index)
    {
        Color selectedColor;
        if (index > cards.Count) //dobierana jest losowa karta z kupki
        {
            selectedColor = MoveCard();
        }
        else // dobierana jest wybrana karta z panelu
        {
            selectedColor = (Color)actualCardColor[index];

            // rozpoczyna siê animacja doboru karty danego koloru przez gracza
            gameManager.SpawnCards(cards[index].transform, actualCardColor[index], names[actualCardColor[index]]);
            gameManager.iSendSpawnCardsServerRpc = true;
            
            // animacja dla pozosta³ych graczy

            gameManager.SpawnCardsServerRpc(cards[index].transform.position, actualCardColor[index], names[actualCardColor[index]]+"BelongToOtherPlayer", index);

            ChooseRandomColor(index);
            if (numberOfRainbowCards >= 3)
                ShuffleCards();
            
        }

        // zapis stanu kart "na rêce" na bie¿¹co
        int[] cardsStacks = new int[gameManager.cardStackCounterList.Count];
        for (int i = 0; i < gameManager.cardStackCounterList.Count; i++)
        {
            cardsStacks[i] = i == (int)selectedColor ? int.Parse(gameManager.cardStackCounterList[i].text) + 1 : int.Parse(gameManager.cardStackCounterList[i].text);
        }
        cardDeck.SendCardsStacksServerRpc(cardsStacks, PlayerGameData.Id);

        return selectedColor;
    }
    
    public void AiDrawCard(int index, ArtificialPlayer ai)
    {
        StartCoroutine(SpawnCardOfIndex(index, ai));
    }

    public IEnumerator SpawnCardOfIndex(int index, ArtificialPlayer ai)
    {
        gameManager.SpawnCardsServerRpc(cards[index].transform.position, actualCardColor[index], names[actualCardColor[index]] + "BelongToOtherPlayer", index);
        ChooseRandomColor(index);

        yield return new WaitForSeconds(3);

        //Communication.EndAITurn(ai);
    }

    public void ChooseRandomColor(int index)
    {
        // wybierany jest nowy kolor i synchronizowany
        int color = 0;
        RandomSprite(ref color);
        if((Color)color == Color.special)
            numberOfRainbowCards++;
        actualCardColor[index] = color;

        // Synchronizacja nowego koloru z innymi graczami
        SyncSpritesServerRpc(color, index);
    }

    public Color[] GetCurrentCardsToChoose()
    {
        return new Color[] { (Color)actualCardColor[0], (Color)actualCardColor[1], 
            (Color)actualCardColor[2], (Color)actualCardColor[3], (Color)actualCardColor[4], };
    }

    private Color MoveCard()
    {
        // gracz dobiera losow¹ kartê z kupki z kartami (widoczne tylko lokalnie)
        int color = 0;
        RandomSprite(ref color);
        gameManager.SpawnCards(drawCardsButton.transform, color, names[color]);
        gameManager.iSendSpawnCardsServerRpc = true;

        // animacja dla pozosta³ych graczy

        gameManager.SpawnCardsServerRpc(drawCardsButton.transform.position, color, names[color] + "BelongToOtherPlayer",-1);

        return (Color)color;
    }
    
    public static bool IsCardRandom(int index)
    {
        return index > 4;
    }

    void ShuffleCards()
    {
        StartCoroutine(HighlightDrawCardsPanel());
        numberOfRainbowCards = 0;
        for(int i = 0;i < cards.Count; i++)
        {
            ChooseRandomColor(i);
        }
        
    }

    IEnumerator HighlightDrawCardsPanel()
    {
        gameObject.GetComponent<Image>().color = UnityEngine.Color.yellow;
        //Debug.Log(GameObject.Find("InfoText").GetComponent<TMP_Text>().text);
        //GameObject.Find("InfoText").GetComponent<TMP_Text>().text = "Przetasowano karty";
        gameManager.SetInfoTextServerRpc("Przetasowano karty");
        //Debug.Log(GameObject.Find("InfoText").GetComponent<TMP_Text>().text);
        yield return new WaitForSeconds(1);
        gameObject.GetComponent<Image>().color = UnityEngine.Color.white;
    }
}
