using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class BuildPath : MonoBehaviour
{
    private GameManager gameManager;
    private Transform[] tilesTransforms;
    private Renderer[] tilesRenderers;
    public Path path;

    // Start is called before the first frame update
    void Start()
    {
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
        //if (PlayerGameData.CanBuildPath(path))
        {
            PlayerGameData.BuildPath(path);
            Communication.BuildPath(this, path);
        }
    }

    public IEnumerator BuildPathAnimation()
    {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            // aktualizacja licznika dostêpnych statków 
            gameManager.spaceshipCounter.text = (int.Parse(gameManager.spaceshipCounter.text) - 1).ToString();

            //aktualizacja licznika kart danego koloru
            var cardCounter = gameManager.cardStackCounterList[(int)path.color];
            cardCounter.text = (int.Parse(cardCounter.text) - 1).ToString();

            // host spawni statki i je przemieszcza 
            gameManager.SpawnShipsServerRpc(tilesTransforms[i + 1].position, tilesTransforms[i + 1].rotation);
            
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
