using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PathScriptableObject", menuName = "ScriptableObjects/Path")]
public class Path : ScriptableObject
{
    //public int Id { get; set; }
    //public int[] planetsIds = new int[2];
    public Planet planetFrom;
    public Planet planetTo;
    public Color color;
    public int length;
    public bool isBuilt { get; set; } = false;
    public PlayerState builtBy = null;
    public bool withSatellie { get; set; } = false;
    public PlayerState playerOfSatellite = null;

    public bool IsEqual(Path other)
    {
        return planetFrom.name == other.planetFrom.name && planetTo.name == other.planetTo.name;
    }

    public Path()
    { }

    public Path(Color color, int length)
    {
        this.color = color;
        this.length = length;
    }
}
