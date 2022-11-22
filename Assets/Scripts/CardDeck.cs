using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    public int[] cards; // liczba kard dla ka¿dego koloru
    public Color[] colors; // kolory kart
    void Start()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int n = cards.Length;

        for (int i = 0; i < n; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().color = colors[i];
            card.transform.GetChild(0).GetComponent<TMP_Text>().text = cards[i].ToString();
        }

        Destroy(cardTempalte);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
