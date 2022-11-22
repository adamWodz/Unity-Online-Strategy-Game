using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject shipGameObject;

    private List<GameObject> shipList = new();
    private List<Transform> shipTransformList = new();
    private Vector3 spaceshipsBase = new(-8, -4, -1);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnShips(Transform t)
    {
        var spawnedShipGameObject = Instantiate(shipGameObject, spaceshipsBase, t.rotation);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.shipGoal = t;
        spawnedShip.move = true;
    }
}
