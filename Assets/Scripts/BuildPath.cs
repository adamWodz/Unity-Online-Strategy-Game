using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildPath : NetworkBehaviour
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
        BuildOnePath();
        //PathBuildServerRpc();
        PathBuildClientRpc();
        Debug.Log("Build path;");
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
    private void PathBuildServerRpc()
    {
        BuildOnePath();
        Debug.Log("Server Rpc;");
    }
    
    [ClientRpc]
    private void PathBuildClientRpc()
    {
        BuildOnePath();
        Debug.Log("Client Rpc");
    }
}
