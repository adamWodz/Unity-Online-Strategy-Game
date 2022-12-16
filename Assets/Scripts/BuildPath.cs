using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildPath : MonoBehaviour
{
    private GameManager gameManager;
    Transform[] tilesTransforms;
    Renderer[] tilesRenderers;
    Path path = new();

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tilesRenderers = gameObject.GetComponentsInChildren<Renderer>();
        tilesTransforms = gameObject.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        PlayerGameData player = PlayerGameData.GetInstance();
        
        if(!player.BuildPath(path))
        {
            return;
        }
        else
        {
            StartCoroutine(SpawningShipsOnPath());
        }
    }

    IEnumerator SpawningShipsOnPath()
    {
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            gameManager.SpawnShipsServerRpc(tilesTransforms[i + 1].position, tilesTransforms[i+1].rotation);
            yield return new WaitForSeconds(0.2f);
        }
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
