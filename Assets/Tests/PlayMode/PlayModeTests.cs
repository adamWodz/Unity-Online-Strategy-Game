using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayModeTests
{
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.

        Server.allPlayersInfo = new List<PlayerInfo>()
        {
            new PlayerInfo
            {
                Id = 0
            }
        };

        PlayerGameData.Id = 0;

        SceneManager.LoadScene("Scenes/Main Game");

        //GameObject.Find("Canvas").transform.Find("OptionButton").GetComponent<Button>().clicked.Invoke();

        yield return new WaitForFixedUpdate();
    }

}
