using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int mapNumber;
    public PlayerInfo actualPlayer;
    public List<PlayerInfo> players;
    public List<Path> paths;
    public SerializableDictionary<int, int[]> cardsForEachPalyer;

    public GameData()
    {
        mapNumber = 0;
        actualPlayer = new();
        players = new();
        cardsForEachPalyer = new();
    }
}
