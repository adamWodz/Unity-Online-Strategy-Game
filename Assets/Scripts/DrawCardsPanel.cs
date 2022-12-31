using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    GameObject card;
    public int[] actualCardColor = new int[5];

    void Start()
    {
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
                child.gameObject.name = names[color];
                child.gameObject.GetComponent<Button>().onClick.AddListener(() => Communication.DrawCard(this, copy));//MoveCard(copy));
                actualCardColor[copy] = color;
                i++;
                cards.Add(child.gameObject);
            }
        }
        drawCardsButton.onClick.AddListener(() => Communication.DrawCard(this,10));
    }

    private void Update()
    {
        // Synchronizacja kolorów kart do dobrania przez hosta
        if(IsHost)
        {
            for(int i = 0;i < cards.Count; i++)
            {
                int copy = i;
                SyncSpritesClientRpc(actualCardColor[copy], copy);
            }
        }
        
    }

    [ClientRpc]
    void SyncSpritesClientRpc(int color, int index)
    {
        ChangeCardColor(color, index);
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncSpritesServerRpc(int color,int index)
    {
        ChangeCardColor(color, index);
    }

    void ChangeCardColor(int color, int index)
    {
        actualCardColor[index] = color;
        cards[index].GetComponent<Image>().sprite = sprites[actualCardColor[index]];
        cards[index].name = names[actualCardColor[index]];
    }
    
    Sprite RandomSprite(ref int index)
    {
        index = UnityEngine.Random.Range(0, sprites.Length);
        return sprites[index];
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
            // rozpoczyna siê animacja doboru karty danego koloru przez gracza
            gameManager.SpawnCards(cards[index].transform, actualCardColor[index], names[actualCardColor[index]]);
            selectedColor = (Color)actualCardColor[index];

            // wybierany jest nowy kolor i synchronizowany
            int color = 0;
            RandomSprite(ref color);
            actualCardColor[index] = color;
            SyncSpritesClientRpc(color, index);
            SyncSpritesServerRpc(color, index);
        }

        return selectedColor;
    }
    
    public Color MoveCard()
    {
        // gracz dobiera losow¹ kartê z kupki z kartami (widoczne tylko lokalnie)
        int color = 0;
        RandomSprite(ref color);
        gameManager.SpawnCards(drawCardsButton.transform, color, names[color]);

        return (Color)color;
    }
    
}
