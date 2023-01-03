using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    public int[] cardsQuantityPerColor; // liczba kart dla ka¿dego koloru
    public Sprite[] sprites; // kolory kart
    // nazwy zbiorów kart
    public string[] names = {"RedCards","GreenCards","BlueCards","YellowCards","PinkCards","RainbowCards"};
    void Start()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int colorsNumber = cardsQuantityPerColor.Length;

        for (int i = 0; i < colorsNumber; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            card.transform.GetChild(0).GetComponent<TMP_Text>().text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            int ii = i;
            card.GetComponent<Button>().onClick.AddListener(() => Communication.ChooseCard((Color)ii));
        }

        Destroy(cardTempalte);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
