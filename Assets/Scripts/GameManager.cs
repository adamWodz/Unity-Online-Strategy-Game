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
        float angle = CalculateAngle(t.position,spaceshipsBase);
        var spawnedShipGameObject = Instantiate(shipGameObject, spaceshipsBase, Quaternion.Euler(new Vector3(0, 0, -angle)));//t.rotation);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.shipGoal = t;
        spawnedShip.move = true;
    }

    public float CalculateAngle(Vector3 position1, Vector3 position2)
    {
        return Mathf.Atan2(
                          position1.x - position2.x,
                          position1.y - position2.y
                          ) * Mathf.Rad2Deg;
    }
}
