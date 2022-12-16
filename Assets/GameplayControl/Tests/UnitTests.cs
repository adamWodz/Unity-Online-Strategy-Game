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
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 2, false)]
        [TestCase(Assets.GameplayControl.Color.blue, 3, Assets.GameplayControl.Color.red, 1, false)]
        public void CanBuildPathWhenNoPlayersTurnTest(Assets.GameplayControl.Color pathColor, int pathLength, Assets.GameplayControl.Color cardsColor, int cardsQuantity, bool expected)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[cardsColor] = cardsQuantity;
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(player.CanBuildPath(path), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 2, true)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.blue, 2, false)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 1, false)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 2, true)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 4, true)]
        public void CanBuildPathTests(Assets.GameplayControl.Color pathColor, int pathLength, Assets.GameplayControl.Color cardsColor, int cardsQuantity, bool expected)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[cardsColor] = cardsQuantity;
            player.NewTurn();
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(player.CanBuildPath(path), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 2, 3, 1)]
        public void BuildPathTests(Assets.GameplayControl.Color color, int length, int enterQuantity, int finalQuantity)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[color] = enterQuantity;
            player.NewTurn();
            Path path = new Path(color, length);
            player.BuildPath(path);
            Assert.AreEqual(player.numOfCardsInColor[color], finalQuantity);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 0, true)]
        [TestCase(Assets.GameplayControl.Color.green, 1, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, true)]
        public void CanSendSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, false)]
        public void CanSendSatellitePlanetWithSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            planet.withSatellite = true;
            Path path = new Path();
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, false)]
        public void CanSendSatellitePathWithSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData player = new PlayerGameData("example", null);
            player.numOfCardsInColor[color] = cardsquantity;
            player.satellitesSent = satellietesSent;
            player.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            path.withSatellie = true;
            Assert.AreEqual(player.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 1, 2, Assets.GameplayControl.Color.white, 2, 3)]
        [TestCase(Assets.GameplayControl.Color.special, 0, 2, Assets.GameplayControl.Color.special, 0, 2)]
        public void DrawCardsTests(Assets.GameplayControl.Color color1, int enterQuantity1, int finalQuantity1, Assets.GameplayControl.Color color2, int enterQuantity2, int finalQuantity2)
        {
            PlayerGameData player = new PlayerGameData("example", null);
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
            Path path12 = new Path(Assets.GameplayControl.Color.red, 2);
            path12.planetFrom = planet1;
            path12.planetTo = planet2;
            Path path23 = new Path(Assets.GameplayControl.Color.green, 3);
            path23.planetFrom = planet2;
            path23.planetTo = planet3;

            PlayerGameData player = new PlayerGameData("example", null);
            player.DrawCards(Assets.GameplayControl.Color.red, Assets.GameplayControl.Color.red);
            player.DrawCards(Assets.GameplayControl.Color.green, Assets.GameplayControl.Color.green);
            player.DrawCards(Assets.GameplayControl.Color.green, Assets.GameplayControl.Color.green);

            player.BuildPath(path12);
            player.BuildPath(path23);

            Assert.IsTrue(mission.IsCompletedByPlayer(player));
        }
    }
}
