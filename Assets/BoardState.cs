using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Color
{
    black,
    red,
    green,
    blue,
    yellow,
    white,
    orange
}

[Serializable]
public struct Planet
{
    public string name;
    public int id;
}



[Serializable]
public class BoardState : MonoBehaviour
{
    List<Planet> planets;

}
