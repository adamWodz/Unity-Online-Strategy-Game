using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

public static class Server
{
    public static List<PlayerInfo> allPlayersInfo { get; set; }
    public static List<ArtificialPlayer> artificialPlayers { get; set; }
    public static List<BuildPath> buildPaths = new List<BuildPath>();
    public static int curPlayerId;
    public static List<Mission> allMissions;
    public static Dictionary<int, List<Mission>> missionsByPlayerId = new Dictionary<int, List<Mission>>();
    public static int connectedPlayersCount;

    public static List<GameObject> playerTilePrefabs;

    public static void Reset()
    {
        buildPaths = new List<BuildPath>();
        missionsByPlayerId = new Dictionary<int, List<Mission>>();
    }
}
