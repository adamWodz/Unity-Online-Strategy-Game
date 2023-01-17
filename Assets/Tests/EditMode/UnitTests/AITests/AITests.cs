using Assets.GameplayControl;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

class MockMapData
{
    public List<Planet> planets;
    public List<Path> paths;
    public List<Mission> missions;

    public MockMapData()
    {
        planets = new List<Planet>()
        {
            new Planet(), // 0
            new Planet(), // 1
            new Planet(), // 2
            new Planet(), // 3
            new Planet(), // 4
            new Planet(), // 5
            new Planet(), // 6
        };

        //paths = new List<Path>
        //{

        //}
    }
}

public class AITests
{
    MockMapData mapData = new MockMapData();
    
    ArtificialPlayer ai = new ArtificialPlayer
    {
        spaceshipsLeft = 10,
        numOfCardsInColor = new Dictionary<Color, int>()
        {
            { Color.pink, 1 },
            { Color.red, 1 },
            { Color.blue, 1 },
            { Color.yellow, 1 },
            { Color.green, 1 },
            { Color.special, 1 },
        },
        /*groupsOfConnectedPlanets = new List<ConnectedPlanets>()
        {
            new List<Planet>
            {
                new Planet()
            }
        }*/
    
};


    [TestCase()]
    public void PickBestMissionsTest()
    {

    }

    public void WhichPathChosen()
    {

    }

    public void CanBuildPath()
    {

    }

    public void BuildPath()
    {

    }

    public void DrawCards()
    {

    }

    public void BestMove()
    {

    }
}
