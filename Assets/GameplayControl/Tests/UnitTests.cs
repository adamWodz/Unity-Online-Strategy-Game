using System.Collections;
using System.Collections.Generic;
using Assets.GameplayControl;
using NUnit.Framework;
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

        [TestCase]
        public void BuildPathTests()
        {

        }

        [TestCase]
        public void CanSendSatelliteTests()
        {

        }

        [TestCase]
        public void SendSatellitesTests()
        {

        }

        [TestCase]
        public void DrawCardsTests()
        {

        }

        [TestCase]
        public void StartTurnTests()
        {

        }

        [TestCase]
        public void EndTurnTests()
        {

        }
    }

    public class MissionTests
    {

    }

    public class BoardTests
    {

    }
}
