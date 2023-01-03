using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public List<Sprite> cardSprites;
    public GameObject shipGameObject;
    public GameObject cardButton;

    private GameObject spawnedCardGameObject;
    private List<GameObject> shipList = new();
    private List<Transform> shipTransformList = new();
    private Vector3 spaceshipsBase = new(-8, -4, -1);
    private TMP_Text spaceshipCounter;
    private TMP_Text satelliteCounter;

    // Start is called before the first frame update
    void Start()
    {
        //canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        spaceshipCounter = GameObject.Find("SpaceshipCounter").GetComponent<TMP_Text>();
        spaceshipCounter.text = "50";

        satelliteCounter = GameObject.Find("SatelliteCounter").GetComponent<TMP_Text>();
        satelliteCounter.text = "3";
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShipsServerRpc(Vector3 position, Quaternion rotation)
    {
        float angle = CalculateAngle(position,spaceshipsBase);
        var spawnedShipGameObject = Instantiate(shipGameObject, spaceshipsBase, Quaternion.Euler(new Vector3(0, 0, -angle)));
        // spawnuje siê dla wszystkich graczy bo network object
        spawnedShipGameObject.GetComponent<NetworkObject>().Spawn(true);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.goalPosition= position;
        spawnedShip.goalRotation= rotation;
        spawnedShip.move = true;
        spaceshipCounter.text = (int.Parse(spaceshipCounter.text) - 1).ToString();
    }

    public void SpawnCards(Transform t, int color, string name)
    {
        cardButton.GetComponent<Image>().sprite = cardSprites[color];
        spawnedCardGameObject = Instantiate(cardButton, t);
        spawnedCardGameObject.name = name;
        var spawnedCard = spawnedCardGameObject.GetComponent<Move>();
        spawnedCard.move = true;
        spawnedCard.speed = 500;
        Transform cardsStack = GameObject.Find(name + "s").GetComponent<Transform>();
        spawnedCard.goalPosition = cardsStack.position;
        spawnedCard.goalRotation = cardsStack.rotation;
    }

    public float CalculateAngle(Vector3 position1, Vector3 position2)
    {
        return Mathf.Atan2(
                          position1.x - position2.x,
                          position1.y - position2.y
                          ) * Mathf.Rad2Deg;
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetBuildPathDataServerRpc(int pathId)
    {
        SetBuildPathDataClientRpc(pathId);
    }

    [ClientRpc]
    public void SetBuildPathDataClientRpc(int pathId)
    {
        PlayerGameData.SetPathIsBuild(pathId);
    }
}
