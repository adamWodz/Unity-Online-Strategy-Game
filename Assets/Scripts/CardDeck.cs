using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    public int[] cards; // liczba kard dla ka¿dego koloru
    public Sprite[] sprites; // kolory kart
    // nazwy zbiorów kart
    public string[] names = {"RedCards","GreenCards","BlueCards","BlackCards","WhiteCards","YellowCards","PinkCards","RainbowCards"};
    void Start()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int n = cards.Length;

        for (int i = 0; i < n; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            card.transform.GetChild(0).GetComponent<TMP_Text>().text = cards[i].ToString();
            card.name = names[i];
        }

        Destroy(cardTempalte);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
