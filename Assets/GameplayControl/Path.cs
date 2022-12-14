using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Path
{
    public int Id { get; set; }
    public int[] planetsIds = new int[2];
    public Planet planetFrom;
    public Planet planetTo;
    public Assets.GameplayControl.Color color;
    public int length;
    public bool isBuilt { get; set; } = false;
    public Player builtBy = null;
    public bool withSatellie { get; set; } = false;
    public Player playerOfSatellite = null;

    public bool IsEqual(Path other)
    {
        return planetFrom.name == other.planetFrom.name && planetTo.name == other.planetTo.name;
    }

    public Path()
    { }

    public Path(Assets.GameplayControl.Color color, int length)
    {
        this.color = color;
        this.length = length;
    }
}
