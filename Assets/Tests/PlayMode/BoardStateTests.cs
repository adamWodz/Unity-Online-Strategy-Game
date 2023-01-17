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
using UnityEngine.UIElements;

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

    /*[UnityTest]
    public IEnumerator NetworkManagerTest()
    {
        NetworkManager.Singleton.StartHost();

        yield return null;
    }*/

    [UnityTest]
    public IEnumerator PlayerTilesNumberTest()
    {
        int playersNum = 2;

        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        SceneManager.LoadScene("Scenes/Main Game");
        yield return new WaitForFixedUpdate();

        int tilesNum = GameObject.Find("Canvas").transform.Find("PlayersPanel").GetComponent<PlayerPanel>().playersTiles.Count;

        Assert.AreEqual(playersNum, tilesNum);
    }

    [UnityTest]
    public IEnumerator StartPlayerSpaceshipNumberTest()
    {
        int playersNum = 2;

        AddPlayers(playersNum);

        PlayerGameData.Id = 0;
        SceneManager.LoadScene("Scenes/Main Game");
        yield return new WaitForFixedUpdate();

        GameObject tile = GameObject.Find("Canvas").transform.Find("PlayersPanel").GetComponent<PlayerPanel>().playersTiles.First();
        int spaceshipsNum = int.Parse(tile.transform.GetChild(3).GetComponent<TMP_Text>().text);
        Assert.AreEqual(Board.startSpaceshipsNumber, spaceshipsNum);
    }

    public IEnumerator DifferentPlayerTilesColorTest()
    {
        yield return new WaitForFixedUpdate();
    }


    public IEnumerator CardsToDrawNumberTest()
    {
        yield return new WaitForFixedUpdate();
    }


}
