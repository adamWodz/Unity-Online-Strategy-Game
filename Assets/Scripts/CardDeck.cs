using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CardDeck : NetworkBehaviour, IDataPersistence
{
    public int[] cardsQuantityPerColor; // liczba kart dla każdego koloru
    public Sprite[] sprites; // kolory kart
    // nazwy zbiorów kart
    public string[] names = {"RedCards","GreenCards","BlueCards","YellowCards","PinkCards","RainbowCards"};

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        GameObject cardTempalte = transform.GetChild(0).gameObject;
        GameObject card;

        int colorsNumber = cardsQuantityPerColor.Length;

        for (int i = 0; i < colorsNumber; i++)
        {
            card = Instantiate(cardTempalte, transform);
            card.GetComponent<Image>().sprite = sprites[i];
            //card.transform.GetChild(0).GetComponent<TMP_Text>().text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            
            var counter = card.transform.GetChild(0).GetComponent<TMP_Text>();
            counter.text = cardsQuantityPerColor[i].ToString();
            card.name = names[i];
            gameManager.cardStackCounterList.Add(counter);

            int ii = i;
            card.GetComponent<Button>().onClick.AddListener(() => Communication.ChooseCard((Color)ii));
        }

        Destroy(cardTempalte);
    }

    public void LoadData(GameData data)
    {
        for(int i=0;i<gameManager.cardStackCounterList.Count;i++)
        {
            gameManager.cardStackCounterList[i].text = data.cardsForEachPalyer[PlayerGameData.Id][i].ToString();
        }
    }

    public void SaveData(ref GameData data)
    {
        int[] numbersOfEachOfCardsColors = new int[gameManager.cardStackCounterList.Count];
        for(int i = 0; i < gameManager.cardStackCounterList.Count; i++)
        {
            numbersOfEachOfCardsColors[i] = int.Parse(gameManager.cardStackCounterList[i].text);
            Debug.Log(numbersOfEachOfCardsColors[i]);
        }
        if (!data.cardsForEachPalyer.ContainsKey(PlayerGameData.Id))
            data.cardsForEachPalyer.Add(PlayerGameData.Id, numbersOfEachOfCardsColors);
        else
            data.cardsForEachPalyer[PlayerGameData.Id] = numbersOfEachOfCardsColors;
    }
}
