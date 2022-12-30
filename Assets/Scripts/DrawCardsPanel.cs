using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DrawCardsPanel : NetworkBehaviour
{
    public Sprite[] sprites; // kolory kart
    public string[] names = { "RedCard", "GreenCard", "BlueCard", "YellowCard", "PinkCard", "RainbowCard" };
    public List<GameObject> cards = new();
    GameManager gameManager;
    Button drawCardsButton;

    void Start()
    {
        drawCardsButton = GameObject.Find("DrawCardsButton").GetComponent<Button>();
        
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        

        //if (IsHost)
        //{
            //Draw();
            
            GameObject cardTempalte = transform.GetChild(0).gameObject;
            GameObject card;
            int n = 5;
            for (int i = 0; i < n; i++)
            {
                int copy = i;
                int index = 0;
                card = Instantiate(cardTempalte, transform);
                //card.GetComponent<NetworkObject>().Spawn(true);
                //card.GetComponent<NetworkObject>().TrySetParent(transform);
                card.GetComponent<Image>().sprite = RandomSprite(ref index);
                card.name = names[index];
                card.GetComponent<Button>().onClick.AddListener(() => MoveCard(copy, ref index));
                cards.Add(card);
            }

            Destroy(cardTempalte);
            drawCardsButton.transform.SetAsLastSibling();
            drawCardsButton.onClick.AddListener(MoveCard);
            
        //}
    }
    [ServerRpc]
    void DrawServerRpc()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int cardsToDrawQuantity = 5;
        for (int i = 0; i < cardsToDrawQuantity; i++)
        {
            int copy = i;
            int index = 0;
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<NetworkObject>().Spawn(true);
            card.GetComponent<NetworkObject>().TrySetParent(transform);
            card.GetComponent<Image>().sprite = RandomSprite(ref index);
            card.name = names[index];
            card.GetComponent<Button>().onClick.AddListener(() => MoveCard(copy, ref index));
            cards.Add(card);
        }

        Destroy(cardTempalte);
        drawCardsButton.transform.SetAsLastSibling();
        drawCardsButton.onClick.AddListener(MoveCard);
    }
    Sprite RandomSprite(ref int index)
    {
        index = UnityEngine.Random.Range(0, sprites.Length);
        return sprites[index];
    }
    private void MoveCard(int index,ref int color)
    {
        Debug.Log($"Index {index}");
        int i = 0;
        gameManager.SpawnCardsServerRpc(cards[index].transform.position, cards[index].transform.rotation, color, cards[index].name);
        //cards[index].SetActive(false);
        //cards.RemoveAt(index);
        cards[index].GetComponent<Image>().sprite = RandomSprite(ref i);
        cards[index].name= names[i];
        color = i;
    }
    
    private void MoveCard()
    {
        int index = 0;
        Sprite sprite = RandomSprite(ref index);
        gameManager.SpawnCardsServerRpc(drawCardsButton.transform.position, drawCardsButton.transform.rotation, index, names[index]);
    }
    
}
