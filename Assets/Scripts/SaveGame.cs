using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPanel;

public class SaveGame : MonoBehaviour
{
    int mapNumber;
    List<Path> paths;
    List<PlayerInfo> playerInfos;
    Queue<GameObject> playerTiles;
    int actualPlayer;
    Dictionary<int, List<Mission>> missionsForEachPlayer;
    Dictionary<int, int[]> cardsForEachPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
