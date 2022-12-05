using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject shipGameObject;
    public GameObject cardButton;

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
        var spawnedShipGameObject = Instantiate(shipGameObject, spaceshipsBase, Quaternion.Euler(new Vector3(0, 0, -angle)));
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.goal = t;
        spawnedShip.move = true;
    }

    public void SpawnCards(Transform t, Sprite sprite, string name)
    {
        cardButton.GetComponent<Image>().sprite = sprite;
        var spawnedCardGameObject = Instantiate(cardButton, t);
        spawnedCardGameObject.name = name;
        var spawnedShip = spawnedCardGameObject.GetComponent<Move>();
        spawnedShip.move = true;
        spawnedShip.speed = 500;
        spawnedShip.goal = GameObject.Find(name+"s").GetComponent<Transform>();
    }

    public float CalculateAngle(Vector3 position1, Vector3 position2)
    {
        return Mathf.Atan2(
                          position1.x - position2.x,
                          position1.y - position2.y
                          ) * Mathf.Rad2Deg;
    }
}
