using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    public int[] cardsQuantity; // liczba kard dla ka¿dego koloru
    public Sprite[] sprites; // kolory kart
    // nazwy zbiorów kart
    public string[] names = {"RedCards","GreenCards","BlueCards","YellowCards","PinkCards","RainbowCards"};

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int colorsNumber = cardsQuantity.Length;

        for (int i = 0; i < colorsNumber; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            var counter = card.transform.GetChild(0).GetComponent<TMP_Text>();
            counter.text = cardsQuantity[i].ToString();
            card.name = names[i];
            gameManager.cardStackCounterList.Add(counter);
        }

        Destroy(cardTempalte);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
