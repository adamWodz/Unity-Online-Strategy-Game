using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public enum PlayerColor 
{
    red,
    green,
    blue,
    yellow,
    white
}

[Serializable]
public class PlayerState : MonoBehaviour
{
    public PlayerColor playerColor;
    public string playerName;
    public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>();

}
