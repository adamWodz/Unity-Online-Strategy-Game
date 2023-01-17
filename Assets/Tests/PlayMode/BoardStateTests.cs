using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Assets.GameplayControl;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BoardStateTests
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

    void StartMainGameScreen(int playersNum)
    {
        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        SceneManager.LoadScene("Scenes/Main Game");
    }

    [UnityTest]
    public IEnumerator PlayerTilesNumberTest()
    {
        int playersNum = 2;
        StartMainGameScreen(playersNum);

        yield return new WaitForFixedUpdate();

        int tilesNum = GameObject.Find("Canvas").transform.Find("PlayersPanel").GetComponent<PlayerPanel>().playersTiles.Count;

        Assert.AreEqual(playersNum, tilesNum);
    }

    [UnityTest]
    public IEnumerator StartPlayerSpaceshipNumberTest()
    {
        StartMainGameScreen(2);

        yield return new WaitForFixedUpdate();

        GameObject tile = GameObject.Find("Canvas").transform.Find("PlayersPanel").GetComponent<PlayerPanel>().playersTiles.First();
        int spaceshipsNum = int.Parse(tile.transform.GetChild(3).GetComponent<TMP_Text>().text);
        Assert.AreEqual(Board.startSpaceshipsNumber, spaceshipsNum);
    }

    [UnityTest]
    public IEnumerator StartCardsNumTest()
    {
        StartMainGameScreen(2);
        yield return new WaitForFixedUpdate();

        CardDeck cardsDeck = GameObject.Find("CardDeck").GetComponent<CardDeck>();
        string[] names = cardsDeck.names;

        foreach(string name in names)
        {
            GameObject cardStack = GameObject.Find(name);
            Assert.IsNotNull(cardStack);
            TMP_Text counter = cardStack.transform.GetChild(0).GetComponent<TMP_Text>();
            int counterValue = int.Parse(counter.text);

            Assert.AreEqual(1, counterValue);
        }
    }

    [UnityTest]
    public IEnumerator DifferentPlayerColorsTest()
    {
        StartMainGameScreen(2);
        yield return new WaitForFixedUpdate();

        var players = GameObject.Find("Canvas").transform.Find("PlayersPanel").GetComponent<PlayerPanel>().players;

        Assert.AreNotEqual(players[0].ColorNum, players[1].ColorNum);
    }

    [UnityTest]
    public IEnumerator CardsToDrawNumberTest()
    {
        StartMainGameScreen(2);
        yield return new WaitForFixedUpdate();

        GameObject cards = GameObject.Find("DrawCardsPanel");

        Assert.AreEqual(5, cards.transform.childCount);
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

        Assert.AreEqual(pom+1, int.Parse(counter.text));
    }
}
