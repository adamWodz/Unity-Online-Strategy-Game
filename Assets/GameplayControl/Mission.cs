﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl
{
    public class Mission
    {
        public Guid Id = Guid.NewGuid();
        public Planet start { get; set; }
        public Planet end { get; set; }
        public int points;
        public bool IsCompletedByPlayer(Player player)
        {
            return ConnectedPlanets.ArePlanetsInOneGroup(player.groupsOfConnectedPlanets, start, end);
        }
    }
}
