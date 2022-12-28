using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Server
{
    public static List<PlayerState> players;
    public static List<ArtificialPlayer> artificialPlayers;
    private static int currentIndex = 0;

    public static void NextTurn()
    {
        PlayerState nextPlayer = players[currentIndex];

        if(nextPlayer.isAI)
        {
            foreach(ArtificialPlayer ai in artificialPlayers)
                if(nextPlayer.id == ai.Id)
                {
                    ai.BestMove();
                    break;
                }
        }
        else
        {
            Communication.StartTurnClientRpc(nextPlayer.id);
        }

        currentIndex = (currentIndex + 1) % players.Count;
    }
}
