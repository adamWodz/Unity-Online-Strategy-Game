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
            ScriptableObject.CreateInstance<Planet>(), // 0
            ScriptableObject.CreateInstance<Planet>(), // 1 
            ScriptableObject.CreateInstance<Planet>(), // 2
            ScriptableObject.CreateInstance<Planet>(), // 3
            ScriptableObject.CreateInstance<Planet>(), // 4
            ScriptableObject.CreateInstance<Planet>(), // 5
            ScriptableObject.CreateInstance<Planet>(), // 6
        };

        paths = new List<Path>
        {
            ScriptableObject.CreateInstance<Path>(), // 0
            ScriptableObject.CreateInstance<Path>(), // 1 
            ScriptableObject.CreateInstance<Path>(), // 2
            ScriptableObject.CreateInstance<Path>(), // 3
            ScriptableObject.CreateInstance<Path>(), // 4
            ScriptableObject.CreateInstance<Path>(), // 5
            ScriptableObject.CreateInstance<Path>(), // 6
        };
        paths[0].planetFrom = planets[0];
        paths[0].planetTo = planets[1];
        paths[0].color = Color.blue;
        paths[0].length = 2;
        paths[1].planetFrom = planets[1];
        paths[1].planetTo = planets[4];
        paths[1].color = Color.blue;
        paths[1].length = 3;
        paths[2].planetFrom = planets[2];
        paths[2].planetTo = planets[4];
        paths[2].color = Color.green;
        paths[2].length = 4;
        paths[3].planetFrom = planets[2];
        paths[3].planetTo = planets[5];
        paths[3].color = Color.green;
        paths[3].length = 1;
        paths[4].planetFrom = planets[2];
        paths[4].planetTo = planets[6];
        paths[4].color = Color.red;
        paths[4].length = 4;
        paths[5].planetFrom = planets[3];
        paths[5].planetTo = planets[4];
        paths[5].color = Color.green;
        paths[5].length = 2;
        paths[6].planetFrom = planets[5];
        paths[6].planetTo = planets[6];
        paths[6].color = Color.yellow;
        paths[6].length = 1;

        missions = new List<Mission>()
        {
            ScriptableObject.CreateInstance<Mission>(), // 0
            ScriptableObject.CreateInstance<Mission>(), // 1 
            ScriptableObject.CreateInstance<Mission>(), // 2
        };

        missions[0].start = planets[0];
        missions[0].end = planets[3];
        missions[1].start = planets[6];
        missions[1].end = planets[3];
        missions[2].start = planets[1];
        missions[2].end = planets[4];
    }
}

public class AITests
{
    MockMapData mockMapData = new MockMapData();

    ArtificialPlayer GetAI()
    {
        return new ArtificialPlayer
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
            groupsOfConnectedPlanets = new List<ConnectedPlanets>()
            {
                new ConnectedPlanets()
                {
                    planets = new List<Planet>
                    {
                        mockMapData.planets[2],
                        mockMapData.planets[6],
                    }
                },
                new ConnectedPlanets()
                {
                    planets = new List<Planet>
                    {
                        mockMapData.planets[3],
                        mockMapData.planets[4],
                    }
                },
            }
        };
    }

    void SetMapData(MockMapData mockMapData)
    {
        MapData data = ScriptableObject.CreateInstance<MapData>();
        data.planets = mockMapData.planets;
        data.paths = mockMapData.paths;
        data.missions = mockMapData.missions;
        Map.mapData = data;
    }

    [Test]
    public void PickBestMissionsTest()
    {
        SetMapData(mockMapData);
        ArtificialPlayer ai = GetAI();

        List<Mission> pickedMissions = ai.PickBestMissions(Map.mapData.missions);

        Assert.IsNotNull(pickedMissions);
        Debug.Log(pickedMissions.Count);
        //Assert.AreEqual(2, pickedMissions.Count);
        //Assert.IsTrue(pickedMissions.Contains(mockMapData.missions[1]));
        //Assert.IsTrue(pickedMissions.Contains(mockMapData.missions[2]));
    }

    public void BestPathToBuild()
    {

    }

    public void CanBuildPathTest()
    {

    }

    public void DrawCards()
    {

    }

     //public void BestMove()
    //{

    //}
}
