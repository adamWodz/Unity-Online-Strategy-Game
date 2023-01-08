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
    public PlayerInfo actualPlayer;
    public List<PlayerInfo> players;
    public List<Path> paths;
    public List<Mission> missionsToChoose;
    public Dictionary<int, int[]> cardsForEachPalyer;
    public Dictionary<int, List<Mission>> missionsForEachPalyer;

    public GameData()
    {
        mapNumber = 0;
        actualPlayer = new();
        players = new();
        paths = new();
        missionsToChoose = new();
        cardsForEachPalyer = new();
        missionsForEachPalyer = new();
    }
}
