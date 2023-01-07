using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public List<Sprite> cardSprites;
    //public GameObject shipGameObject;
    public GameObject cardButton;

    private GameObject spawnedCardGameObject;
    public List<GameObject> shipGameObjectList = new();
    public List<TMP_Text> cardStackCounterList = new();
    private Vector3 spaceshipsBase = new(-8, -4, -1);
    public TMP_Text spaceshipCounter;
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

    [ClientRpc]
    public void TestClientRpc()
    {
        Debug.Log("TestClientRpc;");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnShipsServerRpc(Vector3 position, Quaternion rotation,ServerRpcParams serverRpcParams = default)
    {
        int clientId = (int)serverRpcParams.Receive.SenderClientId;
        float angle = CalculateAngle(position,spaceshipsBase);
        var spawnedShipGameObject = Instantiate(shipGameObjectList[clientId], spaceshipsBase, Quaternion.Euler(new Vector3(0, 0, -angle)));
        // spawnuje siê dla wszystkich graczy bo network object
        spawnedShipGameObject.GetComponent<NetworkObject>().Spawn(true);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.goalPosition = position;
        spawnedShip.goalRotation = rotation;
        spawnedShip.move = true;
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
        Map.mapData.paths.Where(p => p.Id == pathId).FirstOrDefault().isBuilt = true;
    }

    public void SetPopUpWindow(string message)
    {
        var popUp = GameObject.Find("Canvas").transform.GetChild(0);//transform.parent.GetChild(0);
        popUp.GetChild(0).GetComponent<TMP_Text>().text = message;
        popUp.gameObject.SetActive(true);
    }

    public void SetPopUpWindow(string message)
    {
        var popUp = GameObject.Find("Canvas").transform.GetChild(0);
        popUp.GetChild(0).GetComponent<TMP_Text>().text = message;
        popUp.gameObject.SetActive(true);
    }
}
