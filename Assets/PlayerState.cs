using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public enum PlayerColor 
{
    red,
    green,
    blue,
    yellow,
    white
}

[Serializable]
public class PlayerState : MonoBehaviour
{
    public PlayerColor playerColor;
    public string playerName;
    public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>();
    bool isNowPlaying { set; get; }
    int curPoints { set; get; }
    int satellitesLeft { set; get; }


    private bool CanBuildPath(Path path)
    {
        if (!isNowPlaying) return false;
        if(path.isBuilt) return false;
        if (numOfCardsInColor[path.color] < path.length) return false;

        return true;
    }

    public bool BuildPath(Path path)
    {
        if(CanBuildPath(path))
        {
            UpdatePlayerAfterBuild(path);
            return true;
        }
        return false;
    }

    public void UpdatePlayerAfterBuild(Path path)
    {
        curPoints += path.length;
        numOfCardsInColor[path.color] -= path.length;
        path.isBuilt = true;
        
        PropagateChanges();
    }

    public void PropagateChanges()
    {
        // wiadomo�� do serwera �eby powiadomi� pozosta�ych graczy o zmianach
        throw new NotImplementedException();
    }

    public void NewTurn()
    {
        isNowPlaying = true;
    }

    public void EndTurn()
    {
        isNowPlaying = false;
        // wiadomo�� do serwera �eby powiadomi� pozosta�ych graczy o zmianach
        throw new NotImplementedException();
    }

}
