using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject shipGameObject;

    private List<GameObject> shipList = new();
    private List<Transform> shipTransformList = new();

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
        var spawnedShipGameObject = Instantiate(shipGameObject, new Vector3(0, 0, -1), t.rotation);
        var spawnedShip = spawnedShipGameObject.GetComponent<Ship>();
        spawnedShip.shipGoal = t;
        spawnedShip.move = true;
    }
}
