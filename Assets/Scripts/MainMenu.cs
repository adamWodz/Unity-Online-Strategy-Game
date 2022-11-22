using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    public void StartGame()
    {
        // przejœcie do ekranu rozgrywki
        // indeksy scen znajduj¹ siê w 'File->Build Settings'
        SceneManager.LoadScene(1);
        NetworkManager.Singleton.StartHost();
    }
}
