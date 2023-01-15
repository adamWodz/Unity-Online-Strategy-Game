using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EndGame : MonoBehaviour
{
    UnityEngine.UI.Toggle toggle;
    public List<GameObject> otherThanToggle;
    List<GameObject> mapObjects;

    private void Start()
    {
        toggle = gameObject.transform.Find("Toggle").GetComponent<UnityEngine.UI.Toggle>();
        mapObjects = GameObject.Find("GameManager").GetComponent<GameManager>().spawnedObjects;
    }

    public void ToggleValueChanged()
    {
        if(toggle.isOn)
        {
            foreach(GameObject obj in otherThanToggle)
                obj.SetActive(false);

            foreach (GameObject obj in mapObjects)
                if (obj != null)
                    obj.SetActive(true);
        }

        else
        {
            foreach (GameObject obj in otherThanToggle)
                obj.SetActive(true);

            foreach (GameObject obj in mapObjects)
                if (obj != null)
                    obj.SetActive(false);
        }
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        
        Application.Quit();

        /*string name = "Scenes/Menu";
        SceneManager.LoadScene(name, LoadSceneMode.Single);*/
    }
}
