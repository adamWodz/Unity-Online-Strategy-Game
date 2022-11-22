using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PathBuilder : NetworkBehaviour
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

    private void BuildOnePath()
    {
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].material.color = Color.blue;
            gameManager.SpawnShips(objects[i + 1]);
        }
    }

    [ServerRpc]
    public void PathBuildServerRpc()
    {
        BuildOnePath();
        Debug.Log("Server Rpc;");
    }

    [ClientRpc]
    public void PathBuildClientRpc()
    {
        BuildOnePath();
        Debug.Log("Client Rpc");
    }
}
