using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;

public static class Server
{
    public static List<PlayerInfo> allPlayersInfo { get; set; }
    public static List<ArtificialPlayer> artificialPlayers { get; set; }
    public static int curPlayerId = 1;
}
