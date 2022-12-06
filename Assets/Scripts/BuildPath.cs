using System.Collections;
using System.Collections.Generic;
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
        path.isBuilt = false;
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
        if (!path.isBuilt)
        {
            StartCoroutine(Coroutine());
            path.isBuilt = true;
        }
    }

    IEnumerator Coroutine()
    {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < tilesRenderers.Length; i++)
        {
            gameManager.SpawnShips(tilesTransforms[i + 1]);
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
