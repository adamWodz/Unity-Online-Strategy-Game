using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int mapNumber;
    public List<PlayerInfoData> players;
    public List<PathData> paths;
    public Dictionary<int, int[]> cardsForEachPalyer;
    public Dictionary<int, List<MissionData>> missionsForEachPalyer;

    public GameData()
    {
        mapNumber = 0;
        players = new();
        paths = new();
        cardsForEachPalyer = new();
        missionsForEachPalyer = new();
    }
}
