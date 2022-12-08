using System.Collections;
using System.Collections.Generic;
using Assets.GameplayControl;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests
{
    public class PlayerTests
    {
        [TestCase(Color.red, 2, Color.red, 2, false)]
        [TestCase(Color.blue, 3, Color.red, 1, false)]
        public void CanBuildPathWhenNoPlayersTurnTest(Color pathColor, int pathLength, Color cardsColor, int cardsQuantity, bool expected)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[cardsColor] = cardsQuantity;
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(player.CanBuildPath(path), expected);
        }

        [TestCase(Color.red, 2, Color.red, 2, true)]
        [TestCase(Color.red, 2, Color.blue, 2, false)]
        [TestCase(Color.red, 2, Color.red, 1, false)]
        [TestCase(Color.red, 2, Color.red, 2, true)]
        [TestCase(Color.red, 2, Color.red, 4, true)]
        public void CanBuildPathTests(Color pathColor, int pathLength, Color cardsColor, int cardsQuantity, bool expected)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[cardsColor] = cardsQuantity;
            player.NewTurn();
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(player.CanBuildPath(path), expected);
        }

        [TestCase(Color.red, 2, 3, 1)]
        public void BuildPathTests(Color color, int length, int enterQuantity, int finalQuantity)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[color] = enterQuantity;
            player.NewTurn();
            Path path = new Path(color, length);
            player.BuildPath(path);
            Assert.AreEqual(player.numOfCardsInColor[color], finalQuantity);
        }

        [TestCase(Color.green, 3, 0, true)]
        [TestCase(Color.green, 1, 3, false)]
        [TestCase(Color.green, 3, 3, false)]
        [TestCase(Color.green, 0, 0, false)]
        [TestCase(Color.green, 3, 2, true)]
        public void CanSendSatelliteTests(Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Color.green, 3, 3, false)]
        [TestCase(Color.green, 0, 0, false)]
        [TestCase(Color.green, 3, 2, false)]
        public void CanSendSatellitePlanetWithSatelliteTests(Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            planet.withSatellite = true;
            Path path = new Path();
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Color.green, 3, 3, false)]
        [TestCase(Color.green, 0, 0, false)]
        [TestCase(Color.green, 3, 2, false)]
        public void CanSendSatellitePathWithSatelliteTests(Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            path.withSatellie = true;
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Color.red, 1, 2, Color.white, 2, 3)]
        [TestCase(Color.special, 0, 2, Color.special, 0, 2)]
        public void DrawCardsTests(Color color1, int enterQuantity1, int finalQuantity1, Color color2, int enterQuantity2, int finalQuantity2)
        {
            Player player = new Player("example", null);
            player.numOfCardsInColor[color1] = enterQuantity1;
            player.numOfCardsInColor[color2] = enterQuantity2;
            player.DrawCards(color1, color2);
            Assert.AreEqual(player.numOfCardsInColor[color1], finalQuantity1);
            Assert.AreEqual(player.numOfCardsInColor[color2], finalQuantity2);
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
            Path path12 = new Path(Color.red, 2);
            path12.planetFrom = planet1;
            path12.planetTo = planet2;
            Path path23 = new Path(Color.green, 3);
            path23.planetFrom = planet2;
            path23.planetTo = planet3;
            
            Player player = new Player("example", null);
            player.DrawCards(Color.red, Color.red);
            player.DrawCards(Color.green, Color.green);
            player.DrawCards(Color.green, Color.green);

            player.BuildPath(path12);
            player.BuildPath(path23);

            Assert.IsTrue(mission.IsCompletedByPlayer(player));
        }
    }
}
