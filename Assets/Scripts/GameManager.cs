using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject ship;
    public bool spawn = false;

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
        var pom = Instantiate(ship, new Vector3(0, 0, -1), t.rotation);
        var pom2 = pom.GetComponent<Ship>();
        pom2.shipGoal = t;
        pom2.move = true;
    }
}
