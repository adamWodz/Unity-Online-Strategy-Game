using System.Collections;
using System.Collections.Generic;
using Assets.GameplayControl;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

    public class PlayerTests
    {
        [TestCase(Color.red, 2, Color.red, 2, false)]
        [TestCase(Color.blue, 3, Color.red, 1, false)]
        public void CanBuildPathWhenNoPlayersTurnTest(Color pathColor, int pathLength, Color cardsColor, int cardsQuantity, bool expected)
        {
            PlayerGameData.EndTurn();
            PlayerGameData.numOfCardsInColor[cardsColor] = cardsQuantity;
            Path path = new Path(pathColor, pathLength);
            string errorMessage;
            Assert.AreEqual(expected, PlayerGameData.CanBuildPath(path, out errorMessage));
        }

        [TestCase(Color.red, 2, Color.red, 2, true)]
        [TestCase(Color.red, 4, Color.blue, 2, false)]
        [TestCase(Color.red, 3, Color.red, 1, false)]
        [TestCase(Color.red, 2, Color.red, 2, true)]
        [TestCase(Color.red, 2, Color.red, 4, true)]
        public void CanBuildPathTests(Color pathColor, int pathLength, Color cardsColor, int cardsQuantity, bool expected)
        {
            PlayerGameData.numOfCardsInColor[cardsColor] = cardsQuantity;
            PlayerGameData.StartTurn();
            Path path = ScriptableObject.CreateInstance<Path>();
            path.color = pathColor;
            path.length = pathLength;
            string errorMessage;
            Assert.AreEqual(expected, PlayerGameData.CanBuildPath(path, out errorMessage));
        }

        [TestCase(Color.red, 2, 3, 1)]
        public void BuildPathTests(Color color, int length, int enterQuantity, int finalQuantity)
        {
            PlayerGameData.numOfCardsInColor[color] = enterQuantity;
            PlayerGameData.StartTurn();
            Path path = ScriptableObject.CreateInstance<Path>();
            path.color = color;
            path.length = length;
            PlayerGameData.BuildPath(path);
            Assert.AreEqual(finalQuantity, PlayerGameData.numOfCardsInColor[color]);
        }

        [TestCase(Color.green, 3, 0, true)]
        [TestCase(Color.green, 1, 3, false)]
        [TestCase(Color.green, 3, 3, false)]
        [TestCase(Color.green, 0, 0, false)]
        [TestCase(Color.green, 3, 2, true)]
        public void CanSendSatelliteTests(Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData.numOfCardsInColor[color] = cardsquantity;
            PlayerGameData.satellitesSent = satellietesSent;
            PlayerGameData.StartTurn();
            Planet planet = new Planet();
            Path path = ScriptableObject.CreateInstance<Path>();
            Assert.AreEqual(expected, PlayerGameData.CanSendSatellite(planet, path, color));
        }

        [TestCase(Color.green, 3, 3, false)]
        [TestCase(Color.green, 0, 0, false)]
        [TestCase(Color.green, 3, 2, false)]
        public void CanSendSatellitePlanetWithSatelliteTests(Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData.numOfCardsInColor[color] = cardsquantity;
            PlayerGameData.satellitesSent = satellietesSent;
            PlayerGameData.StartTurn();
            Planet planet = new Planet();
            planet.withSatellite = true;
            Path path = ScriptableObject.CreateInstance<Path>();
            Assert.AreEqual(expected, PlayerGameData.CanSendSatellite(planet, path, color));
        }

        [TestCase(Color.red, 1, 2, Color.blue, 2, 3)]
        [TestCase(Color.special, 0, 2, Color.special, 0, 2)]
        public void DrawCardsTests(Color color1, int enterQuantity1, int finalQuantity1, Color color2, int enterQuantity2, int finalQuantity2)
        {
            PlayerGameData.numOfCardsInColor[color1] = enterQuantity1;
            PlayerGameData.numOfCardsInColor[color2] = enterQuantity2;
            PlayerGameData.DrawCards(color1, color2);
            Assert.AreEqual(finalQuantity1, PlayerGameData.numOfCardsInColor[color1]);
            Assert.AreEqual(finalQuantity2, PlayerGameData.numOfCardsInColor[color2]);
        }
    }

    public class MissionTests
    {
        [Test]
        public void IsMissionCompletedTest()
        {
            Planet planet1 = new Planet();
            Planet planet2 = new Planet();
            Planet planet3 = new Planet();
            Mission mission = new Mission();
            mission.start = planet1;
            mission.end = planet3;
            Path path12 = ScriptableObject.CreateInstance<Path>();
            path12.color = Color.red;
            path12.length = 2;
            path12.planetFrom = planet1;
            path12.planetTo = planet2;
            Path path23 = ScriptableObject.CreateInstance<Path>();
            path23.color = Color.green;
            path23.length = 3;
            path23.planetFrom = planet2;
            path23.planetTo = planet3;

            PlayerGameData.DrawCards(Color.red, Color.red);
            PlayerGameData.DrawCards(Color.green, Color.green);
            PlayerGameData.DrawCards(Color.green, Color.green);

            PlayerGameData.BuildPath(path12);
            PlayerGameData.BuildPath(path23);

            Assert.IsTrue(mission.IsCompletedByClientPlayer());
        }
    }

public class ConnectedPlanetsTests
{
    Planet[] planets =
    {
        new Planet(), // 0
        new Planet(), // 1
        new Planet(), // 2
        new Planet(), // 3
        new Planet(), // 4
        new Planet(), // 5
        new Planet(), // 6, nie nale¿y do ¿adnej grupy
    };
    ConnectedPlanets group1, group2, group3;
    List<ConnectedPlanets> groups;

    void ResetGroups()
    {
        group1 = new ConnectedPlanets(); // 0
        group2 = new ConnectedPlanets(); // 1
        group3 = new ConnectedPlanets(); // 2

        group1.planets.Add(planets[0]);
        group1.planets.Add(planets[1]);

        group2.planets.Add(planets[2]);

        group3.planets.Add(planets[3]);
        group3.planets.Add(planets[4]);
        group3.planets.Add(planets[5]);

        groups = new List<ConnectedPlanets>();
        groups.Add(group1);
        groups.Add(group2);
        groups.Add(group3);
    }

    [TestCase(0, 1, true)]
    [TestCase(4, 5, true)]
    [TestCase(1, 2, false)]
    [TestCase(6, 1, false)]
    public void ArePlanetsInOneGroupTests(int ind1, int ind2, bool expected)
    {
        ResetGroups();
        Assert.AreEqual(expected, ConnectedPlanets.ArePlanetsInOneGroup(groups, planets[ind1], planets[ind2]));
    }

    [TestCase(1, 2, 0, 2, true)]
    [TestCase(6, 1, 0, 6, true)]
    [TestCase(6, 2, 6, 2, true)]
    [TestCase(6, 2, 6, 1, false)]
    [TestCase(5, 4, 5, 4, true)]
    public void AddPlanetsFromPathToPlanetsGroups(int planetToInd, int planetFromInd, int planet1ind, int planet2ind, bool expected)
    {
        ResetGroups();
        Path path = ScriptableObject.CreateInstance<Path>();
        path.planetFrom = planets[planetFromInd];
        path.planetTo = planets[planetToInd];
        ConnectedPlanets.AddPlanetsFromPathToPlanetsGroups(path, groups);
        Assert.AreEqual(expected, ConnectedPlanets.ArePlanetsInOneGroup(groups, planets[planet1ind], planets[planet2ind]));
    }

    [TestCase(0, 1, 3)]
    [TestCase(0, 2, 5)]
    public void MergeGroupsTest(int groupInd1, int groupInd2, int expectedCount)
    {
        ResetGroups();
        Assert.AreEqual(expectedCount, ConnectedPlanets.MergeGroups(groups[groupInd1], groups[groupInd2]).planets.Count);
    }
}
