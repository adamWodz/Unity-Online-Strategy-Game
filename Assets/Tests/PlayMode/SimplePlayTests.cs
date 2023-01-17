using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Assets.GameplayControl;
using NUnit.Framework;
using TMPro;
//using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SimplePlayTests
{   
    void AddPlayers(int playersNum)
    {
        Server.allPlayersInfo = new List<PlayerInfo>();
        for (int i = 0; i < playersNum; i++)
        {
            Server.allPlayersInfo.Add(new PlayerInfo
            {
                Id = i,
                Position = i,
                ColorNum = i,
                SpaceshipsLeft = Board.startSpaceshipsNumber
            });
        }
    }
    
    [UnityTest]
    public IEnumerator DrawCardsTest()
    {
        int playersNum = 2;

        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        PlayerGameData.StartTurn();
        
        SceneManager.LoadScene("Scenes/Main Game");
        yield return new WaitForFixedUpdate();

        GameObject card = GameObject.Find("DrawCardsPanel").transform.GetChild(0).gameObject;
        Assert.IsNotNull(card);
        Debug.Log(card.name);
        yield return new WaitForFixedUpdate();
        GameObject cardStack = GameObject.Find(card.name + "s");
        Assert.IsNotNull(cardStack);
        TMP_Text counter = cardStack.transform.GetChild(0).GetComponent<TMP_Text>();
        int pom = int.Parse(counter.text);

        Button button = card.transform.GetComponent<Button>();
        Assert.IsNotNull(button);
        button.onClick.Invoke();

        yield return new WaitForSeconds(2);

        Assert.AreEqual(pom + 1, int.Parse(counter.text));
    }

    [UnityTest]
    public IEnumerator DrawNewMissionTest()
    {
        int playersNum = 2;

        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        PlayerGameData.StartTurn();

        SceneManager.LoadScene("Scenes/Main Game");
        yield return new WaitForFixedUpdate();

        Button drawMissionsCardsButton = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        Assert.IsNotNull(drawMissionsCardsButton);
        yield return new WaitForFixedUpdate();
        
        drawMissionsCardsButton.onClick.Invoke();
        yield return new WaitForSeconds(1);

        var popUp = GameObject.Find("PopUpMissionsDrawing");
        Assert.IsNotNull(popUp);

        Button yesButton = popUp.transform.GetChild(1).GetComponent<Button>();
        Assert.IsNotNull(yesButton);

        yesButton.onClick.Invoke();
        yield return new WaitForSeconds(1);

        GameObject missionsPanel = GameObject.Find("MissionsPanel");
        Assert.IsNotNull(missionsPanel);

        //int numberOfMissionsToChoose = missionsPanel.GetComponent<MissionsPanel>().missionsToChoose.Count;

        Button missionCard = missionsPanel.transform.GetChild(0).GetComponent<Button>();
        Assert.IsNotNull(missionCard);

        Button confirmButton = missionsPanel.transform.GetChild(3).GetComponent<Button>();
        Assert.IsNotNull(confirmButton);

        string firstPlanet = missionCard.transform.GetChild(0).GetComponent<TMP_Text>().text;
        string secondPlanet = missionCard.transform.GetChild(1).GetComponent<TMP_Text>().text;
        int points = int.Parse(missionCard.transform.GetChild(2).GetComponent<TMP_Text>().text);

        missionCard.onClick.Invoke();
        yield return new WaitForFixedUpdate();
        confirmButton.onClick.Invoke();
        yield return new WaitForSeconds(1);

        GameObject pathsPanel = GameObject.Find("PathsPanel");
        Assert.IsNotNull(pathsPanel);

        Transform addedMission = pathsPanel.transform.GetChild(0);
        Assert.IsNotNull(addedMission);

        string firstPlanetAddedMission = addedMission.GetChild(0).GetComponent<TMP_Text>().text;
        string secondPlanetAddedMission = addedMission.GetChild(1).GetComponent<TMP_Text>().text;
        int pointsAddedMission = int.Parse(addedMission.GetChild(2).GetComponent<TMP_Text>().text);

        Assert.AreEqual(firstPlanet, firstPlanetAddedMission);
        Assert.AreEqual(secondPlanet, secondPlanetAddedMission);
        Assert.AreEqual(points, pointsAddedMission);

        //Assert.AreEqual(numberOfMissionsToChoose - 1, missionsPanel.GetComponent<MissionsPanel>().missionsToChoose.Count);
    }

    [UnityTest]
    public IEnumerator PlanetsHighlighTest()
    {
        int playersNum = 2;

        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        PlayerGameData.StartTurn();

        SceneManager.LoadScene("Scenes/Main Game");
        yield return new WaitForFixedUpdate();

        Button drawMissionsCardsButton = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        Assert.IsNotNull(drawMissionsCardsButton);
        yield return new WaitForFixedUpdate();

        drawMissionsCardsButton.onClick.Invoke();
        yield return new WaitForSeconds(1);

        var popUp = GameObject.Find("PopUpMissionsDrawing");
        Assert.IsNotNull(popUp);

        Button yesButton = popUp.transform.GetChild(1).GetComponent<Button>();
        Assert.IsNotNull(yesButton);

        yesButton.onClick.Invoke();
        yield return new WaitForSeconds(1);

        GameObject missionsPanel = GameObject.Find("MissionsPanel");
        Assert.IsNotNull(missionsPanel);

        Button missionCard = missionsPanel.transform.GetChild(0).GetComponent<Button>();
        Assert.IsNotNull(missionCard);

        string firstPlanet = missionCard.transform.GetChild(0).GetComponent<TMP_Text>().text;
        string secondPlanet = missionCard.transform.GetChild(1).GetComponent<TMP_Text>().text;

        missionCard.onClick.Invoke();

        yield return new WaitForFixedUpdate();

        var planet1 = GameObject.Find(firstPlanet);
        Assert.IsNotNull(planet1);

        var planet2 = GameObject.Find(secondPlanet);
        Assert.IsNotNull(planet2);

        bool isHighlighted = planet1.transform.GetChild(0).gameObject.activeSelf && planet1.GetComponent<Renderer>().material.color == UnityEngine.Color.green;
        Assert.IsTrue(isHighlighted);

        bool isHighlighted2 = planet2.transform.GetChild(0).gameObject.activeSelf && planet2.GetComponent<Renderer>().material.color == UnityEngine.Color.green;
        Assert.IsTrue(isHighlighted2);
    }
}
