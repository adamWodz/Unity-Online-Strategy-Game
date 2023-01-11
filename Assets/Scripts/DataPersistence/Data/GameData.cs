using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int mapNumber;
    public int curPlayerId;
    public List<PlayerInfo> players;
    public List<PathData> paths;
    //public List<Mission> missionsToChoose;
    public Dictionary<int, int[]> cardsForEachPalyer;
    public Dictionary<int, List<MissionData>> missionsForEachPalyer;

    public GameData()
    {
        mapNumber = 0;
        curPlayerId = 0;
        players = new();
        paths = new();
        //missionsToChoose = new();
        cardsForEachPalyer = new();
        missionsForEachPalyer = new();
    }
}
