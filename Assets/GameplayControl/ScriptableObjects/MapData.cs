using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapScriptableObject", menuName = "ScriptableObjects/MapData")]
public class MapData : ScriptableObject
{
    public new string name;
    public List<Planet> planets;
    public List<Path> paths;
    public List<Mission> missions;
}
