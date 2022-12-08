using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class UserLogin : MonoBehaviour
{
    public Text profileText;
    public InputField userName;
    public Button loginButton; //authenticate
    public GameObject regularMenu, dropdownMenu;
    public bool requireLogin = true;
    public bool showMenu = true;
    public static string playerName;
    public void UserClick()
    {
        if (requireLogin) showMenu = true;
        regularMenu.SetActive(!showMenu);
        dropdownMenu.SetActive(showMenu);

        ProfileView();

        if (!requireLogin) showMenu = !showMenu;        
    }

    public void ProfileView()
    {
        bool withInput = requireLogin && showMenu;
        bool noInput = !requireLogin && showMenu;
        userName.gameObject.SetActive(withInput);
        loginButton.gameObject.SetActive(withInput);
        profileText.gameObject.SetActive(noInput);
    }
    public void LoginSubmit() {
        playerName = userName.text;
        Debug.Log(playerName);
        if (playerName.Length > 0)
        {
            profileText.text = $"Witaj, {playerName} !";
            requireLogin = false;
            ProfileView();
            LobbySetup.Instance.Authenticate(playerName);
        }
    }
}
