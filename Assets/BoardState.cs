using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public enum Color
{
    black,
    red,
    green,
    blue,
    yellow,
    white,
    orange,
    optional,
    special
}

[Serializable]
public class Planet
{
    public Guid Id { get; } = Guid.NewGuid();
    public string name;
    public Vector3 position;
    public bool withSatellite { set; get; } = false;
    public List<Path> adjacentPaths;
}

[Serializable]
public class Path
{
    public Guid Id { get; } = Guid.NewGuid();
    public int[] planetsIds = new int[2] { 0, 0};
    public Planet planetFrom;
    public Planet planetTo;
    public Color color { get; }
    public int length { get; }
    public bool isBuilt { get; set; } = false;
    public Player builtBy = null;
    public bool withSatellie { get; set; } = false;
    public Player playerOfSatellite = null;

    public Path(int length = 1, Color color = Color.optional)
    {
        this.length = length;
        this.color = color;
    }

    public void SetPlanetsIds()
    {
        // to do
    }
}

