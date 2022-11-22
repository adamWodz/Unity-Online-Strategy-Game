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
        // przej�cie do ekranu rozgrywki
        // indeksy scen znajduj� si� w 'File->Build Settings'
        SceneManager.LoadScene(1);
        NetworkManager.Singleton.StartHost();
    }
}
