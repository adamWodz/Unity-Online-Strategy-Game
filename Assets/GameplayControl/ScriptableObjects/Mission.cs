using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameplayControl
{
    [Serializable]
    [CreateAssetMenu(fileName = "MissionScriptableObject", menuName = "ScriptableObjects/Mission")]
    public class Mission : ScriptableObject
    {
        public Planet start;
        public Planet end;
        public int points;
        public bool isDone = false;
        public int id;
        public bool IsCompletedByPlayer()
        {
            return ConnectedPlanets.ArePlanetsInOneGroup(PlayerGameData.groupsOfConnectedPlanets, start, end);
        }
    }

    [Serializable]
    public struct MissionData
    {
        public string startPlanetName;
        public string endPlanetName;
        public int points;
    }
}
