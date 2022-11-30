using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPath : MonoBehaviour
{
    private GameManager gameManager;
    Transform[] objects;
    Renderer[] paths;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        paths = gameObject.GetComponentsInChildren<Renderer>();
        objects = gameObject.GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine()
    {
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].material.color = UnityEngine.Color.blue;
            gameManager.SpawnShips(objects[i + 1]);
            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
