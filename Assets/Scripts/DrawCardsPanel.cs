using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawCardsPanel : MonoBehaviour
{
    public Sprite[] sprites; // kolory kart
    public string[] names = { "RedCard", "GreenCard", "BlueCard", "BlackCard", "WhiteCard", "YellowCard", "PinkCard", "RainbowCard" };
    public List<GameObject> cards = new();
    GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int n = 5;
        for (int i = 0; i < n; i++)
        {
            int copy = i;
            int index = 0;
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = RandomSprite(ref index);
            card.name = names[index];
            card.GetComponent<Button>().onClick.AddListener(() => MoveCard(copy));
            cards.Add(card);
        }

        Destroy(cardTempalte);
    }
    private void Update()
    {
        
    }
    Sprite RandomSprite(ref int index)
    {
        index = Random.Range(0, sprites.Length);
        return sprites[index];
    }

    private void MoveCard(int index)
    {
        Debug.Log($"Index {index}");
        int i = 0;
        gameManager.SpawnCards(cards[index].transform, cards[index].GetComponent<Image>().sprite, cards[index].name);
        //cards[index].SetActive(false);
        //cards.RemoveAt(index);
        cards[index].GetComponent<Image>().sprite = RandomSprite(ref i);
        cards[index].name= names[i];
    }
}
