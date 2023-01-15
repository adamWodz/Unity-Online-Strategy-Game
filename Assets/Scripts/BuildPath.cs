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

    // Start is called before the first frame update
    void Start()
    {
        cardDeck = GameObject.Find("CardDeck").GetComponent<CardDeck>();   
        path.isBuilt = false;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tilesRenderers = gameObject.GetComponentsInChildren<Renderer>();
        tilesTransforms = gameObject.GetComponentsInChildren<Transform>();
        
        //PlayerGameData.EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Debug.Log("OnMouseDown");
        Communication.ChoosePath(this);
    }

    public void DoBuildPath(int playerColorNum)
    {
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            // aktualizacja licznika dost�pnych statk�w 
            gameManager.spaceshipCounter.text = (int.Parse(gameManager.spaceshipCounter.text) - 1).ToString();

            //aktualizacja licznika kart danego koloru
            var cardCounter = gameManager.cardStackCounterList[(int)path.color];

            // Jeśli nie mamy już kart danego koloru, to znaczy, że musimy użyć tęczowych
            if (cardCounter.text == "0")
            {
                cardCounter = gameManager.cardStackCounterList[^1];
            }
            cardCounter.text = (int.Parse(cardCounter.text) - 1).ToString();
            
            
        }
        // zapis stanu kart "na rêce" na bie¿¹co
        int[] cardsStacks = new int[gameManager.cardStackCounterList.Count];
        for (int j = 0; j < gameManager.cardStackCounterList.Count; j++)
        {
            cardsStacks[j] = int.Parse(gameManager.cardStackCounterList[j].text);
        }
        Debug.Log(cardsStacks);
        cardDeck.SendCardsStacksServerRpc(cardsStacks,PlayerGameData.Id);

        StartCoroutine(BuildPathAnimation(playerColorNum));
    }

    public void DoBuildPathByAI(int playerColorNum)
    {
        StartCoroutine(BuildPathAnimation(playerColorNum));
    }

    public IEnumerator BuildPathAnimation(int playerColorNum)
    {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            // host spawni statki i je przemieszcza 
            gameManager.SpawnShipsServerRpc(playerColorNum, tilesTransforms[i + 1].position, tilesTransforms[i + 1].rotation);
            
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
