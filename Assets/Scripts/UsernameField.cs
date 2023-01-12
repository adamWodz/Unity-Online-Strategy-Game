using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsernameField : MonoBehaviour
{
    InputField inputField;

    void Start()
    {
        inputField = gameObject.GetComponent<InputField>();
        inputField.text = PlayerGameData.Name;
    }

    void Update()
    {
    }

    public void SetUsername()
    {
        PlayerPrefs.SetString("username", inputField.text);
        PlayerGameData.Name = inputField.text;
        Debug.Log(PlayerPrefs.GetString("username"));
    }
}
