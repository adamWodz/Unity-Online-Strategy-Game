using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PathScriptableObject", menuName = "ScriptableObjects/Path")]
public class Path : ScriptableObject
{
    public int Id;
    //public int[] planetsIds = new int[2];
    public Planet planetFrom;
    public Planet planetTo;
    public Color color;
    public int length;
    public bool isBuilt = false;
    public int builtById;
    //public bool withSatellie { get; set; } = false;
    //public PlayerState playerOfSatellite;

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

    public void Init(int id, Planet planetFrom,Planet planetTo,Color color, int length, bool isBuilt, int builtBuId)
    {
        this.Id = id;
        this.planetFrom = planetFrom;
        this.planetTo = planetTo;
        this.color = color;
        this.length = length;
        this.isBuilt = isBuilt;
        this.builtById = builtBuId;
    }
    public static Path CreateInstance(int id, Planet planetFrom, Planet planetTo, Color color, int length, bool isBuilt, int builtBuId)
    {
        Path path = CreateInstance<Path>();
        path.Init(id, planetFrom, planetTo, color, length, isBuilt, builtBuId);
        return path;
    }
}

[Serializable]
public struct PathData
{
    public int id;
    public string planetFromName;
    public string planetToName;
    public Color color;
    public int length;
    public bool isBuilt;
    public int builtById;
}
