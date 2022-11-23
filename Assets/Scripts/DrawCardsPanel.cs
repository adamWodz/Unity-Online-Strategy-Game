using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawCardsPanel : MonoBehaviour
{
    public Color[] colors; // kolory kart
    void Start()
    {
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int n = 5;

        for (int i = 0; i < n; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().color = RandomColor();
        }

        Destroy(cardTempalte);
    }
    Color RandomColor()
    {
        int index = Random.Range(0, colors.Length);
        return colors[index];
    }
}
