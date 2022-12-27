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
            PlayerGameData.numOfCardsInColor[cardsColor] = cardsQuantity;
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(PlayerGameData.CanBuildPath(path), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 2, true)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.blue, 2, false)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 1, false)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 2, true)]
        [TestCase(Assets.GameplayControl.Color.red, 2, Assets.GameplayControl.Color.red, 4, true)]
        public void CanBuildPathTests(Assets.GameplayControl.Color pathColor, int pathLength, Assets.GameplayControl.Color cardsColor, int cardsQuantity, bool expected)
        {
            PlayerGameData.numOfCardsInColor[cardsColor] = cardsQuantity;
            PlayerGameData.NewTurn();
            Path path = new Path(pathColor, pathLength);
            Assert.AreEqual(PlayerGameData.CanBuildPath(path), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 2, 3, 1)]
        public void BuildPathTests(Assets.GameplayControl.Color color, int length, int enterQuantity, int finalQuantity)
        {
            PlayerGameData.numOfCardsInColor[color] = enterQuantity;
            PlayerGameData.NewTurn();
            Path path = new Path(color, length);
            PlayerGameData.PerformBuildPath(path);
            Assert.AreEqual(PlayerGameData.numOfCardsInColor[color], finalQuantity);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 0, true)]
        [TestCase(Assets.GameplayControl.Color.green, 1, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, true)]
        public void CanSendSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData.numOfCardsInColor[color] = cardsquantity;
            PlayerGameData.satellitesSent = satellietesSent;
            PlayerGameData.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            Assert.AreEqual(PlayerGameData.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, false)]
        public void CanSendSatellitePlanetWithSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData.numOfCardsInColor[color] = cardsquantity;
            PlayerGameData.satellitesSent = satellietesSent;
            PlayerGameData.NewTurn();
            Planet planet = new Planet();
            planet.withSatellite = true;
            Path path = new Path();
            Assert.AreEqual(PlayerGameData.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.green, 3, 3, false)]
        [TestCase(Assets.GameplayControl.Color.green, 0, 0, false)]
        [TestCase(Assets.GameplayControl.Color.green, 3, 2, false)]
        public void CanSendSatellitePathWithSatelliteTests(Assets.GameplayControl.Color color, int cardsquantity, int satellietesSent, bool expected)
        {
            PlayerGameData.numOfCardsInColor[color] = cardsquantity;
            PlayerGameData.satellitesSent = satellietesSent;
            PlayerGameData.NewTurn();
            Planet planet = new Planet();
            Path path = new Path();
            path.withSatellie = true;
            Assert.AreEqual(PlayerGameData.CanSendSatellite(planet, path, color), expected);
        }

        [TestCase(Assets.GameplayControl.Color.red, 1, 2, Assets.GameplayControl.Color.white, 2, 3)]
        [TestCase(Assets.GameplayControl.Color.special, 0, 2, Assets.GameplayControl.Color.special, 0, 2)]
        public void DrawCardsTests(Assets.GameplayControl.Color color1, int enterQuantity1, int finalQuantity1, Assets.GameplayControl.Color color2, int enterQuantity2, int finalQuantity2)
        {
            PlayerGameData.numOfCardsInColor[color1] = enterQuantity1;
            PlayerGameData.numOfCardsInColor[color2] = enterQuantity2;
            PlayerGameData.DrawCards(color1, color2);
            Assert.AreEqual(PlayerGameData.numOfCardsInColor[color1], finalQuantity1);
            Assert.AreEqual(PlayerGameData.numOfCardsInColor[color2], finalQuantity2);
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

            PlayerGameData.DrawCards(Assets.GameplayControl.Color.red, Assets.GameplayControl.Color.red);
            PlayerGameData.DrawCards(Assets.GameplayControl.Color.green, Assets.GameplayControl.Color.green);
            PlayerGameData.DrawCards(Assets.GameplayControl.Color.green, Assets.GameplayControl.Color.green);

            PlayerGameData.PerformBuildPath(path12);
            PlayerGameData.PerformBuildPath(path23);

            Assert.IsTrue(mission.IsCompletedByPlayer());
        }
    }
}
