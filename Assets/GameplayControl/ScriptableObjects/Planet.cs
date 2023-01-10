using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PlanetScriptableObject", menuName = "ScriptableObjects/Planet")]
public class Planet : ScriptableObject
{
    public new string name;
    //public int Id { get; set; }
    //public Vector3 position { get; set; }
    public float positionX;
    public float positionY;
    public bool withSatellite { set; get; } = false;
    public List<Path> adjacentPaths { get; set; }
}
