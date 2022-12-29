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
        public Planet start { get; set; }
        public Planet end { get; set; }
        public int points;
        public bool IsCompletedByPlayer()
        {
            return ConnectedPlanets.ArePlanetsInOneGroup(PlayerGameData.groupsOfConnectedPlanets, start, end);
        }
    }
}
