using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    /*
    int mapNumber;
    List<Path> paths;
    List<PlayerInfo> playerInfos;
    Queue<GameObject> playerTiles;
    int actualPlayer;
    Dictionary<int, List<Mission>> missionsForEachPlayer;
    Dictionary<int, int[]> cardsForEachPlayer;
    */
    public int mapNumber;
    public int curPlayerId;
    public List<PlayerInfo> players;
    public List<Path> paths;
    //public List<Mission> missionsToChoose;
    public Dictionary<int, string> cardsForEachPalyer;
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
