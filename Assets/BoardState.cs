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
public class Path
{
    public int[] planets = new int[2];
    public Color color;
    public int length;
    public bool isBuilt { get; set; }
    
}

[Serializable]
public class BoardState : MonoBehaviour
{
    List<Planet> planets;
    
}
