using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BuildPath : MonoBehaviour
{
    public GameManager gameManager;
    private Transform[] tilesTransforms;
    private Renderer[] tilesRenderers;
    public Path path;
    private CardDeck cardDeck;

    void Start()
    {
        cardDeck = GameObject.Find("CardDeck").GetComponent<CardDeck>();   
        path.isBuilt = false;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tilesRenderers = gameObject.GetComponentsInChildren<Renderer>();
        tilesTransforms = gameObject.GetComponentsInChildren<Transform>();
        
    }

    void Update()
    {
        if(PlayerGameData.isNowPlaying && Communication.chosenPath != null && Communication.chosenPath.path.IsEqualById(path))
        {
            transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        Communication.ChoosePath(this);
    }

    public void DoBuildPath(int playerColorNum)
    {
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            gameManager.spaceshipCounter.text = (int.Parse(gameManager.spaceshipCounter.text) - 1).ToString();

            var cardCounter = gameManager.cardStackCounterList[(int)path.color];

            if (cardCounter.text == "0")
            {
                cardCounter = gameManager.cardStackCounterList[^1];
            }
            cardCounter.text = (int.Parse(cardCounter.text) - 1).ToString();
            
            
        }
        int[] cardsStacks = new int[gameManager.cardStackCounterList.Count];
        for (int j = 0; j < gameManager.cardStackCounterList.Count; j++)
        {
            cardsStacks[j] = int.Parse(gameManager.cardStackCounterList[j].text);
        }
        cardDeck.SendCardsStacksServerRpc(cardsStacks,PlayerGameData.Id);

        StartCoroutine(BuildPathAnimation(playerColorNum));
    }

    public void DoBuildPathByAI(int playerColorNum)
    {
        StartCoroutine(BuildPathAnimation(playerColorNum));
    }

    public IEnumerator BuildPathAnimation(int playerColorNum)
    {
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            gameManager.SpawnShipsServerRpc(playerColorNum, tilesTransforms[i + 1].position, tilesTransforms[i + 1].rotation);
            
            yield return new WaitForSeconds(0.2f);
        }
    }
}
