using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawCardsPanel : MonoBehaviour
{
    public Sprite[] sprites; // kolory kart
    void Start()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int n = 5;

        for (int i = 0; i < n; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = RandomSprite();
        }

        Destroy(cardTempalte);
    }
    Sprite RandomSprite()
    {
        int index = Random.Range(0, sprites.Length);
        return sprites[index];
    }
}
