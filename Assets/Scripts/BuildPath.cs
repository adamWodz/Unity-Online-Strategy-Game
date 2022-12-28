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
    public Path path = new();

    Assets.GameplayControl.Mission mission;

    // Start is called before the first frame update
    void Start()
    {
        path.isBuilt = false;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tilesRenderers = gameObject.GetComponentsInChildren<Renderer>();
        tilesTransforms = gameObject.GetComponentsInChildren<Transform>();

        PlayerGameData.EndTurn();
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
            Communication.BuildPathServerRpc(this, path);
        }


    }

    public IEnumerator BuildPathAnimation()
    {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            gameManager.SpawnShips(tilesTransforms[i + 1].position, tilesTransforms[i+1].rotation);
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
