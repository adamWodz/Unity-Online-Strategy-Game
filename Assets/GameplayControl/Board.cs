using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl
{
    public static class Board
    {
        public static Dictionary<int, int> pointsPerLength = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 2},
            {3, 4},
            {4, 7},
            {5, 10},
            {6, 15},
        };
        public static Dictionary<int, int> cardsPerSatelliteSend = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 3},
            {3, 3},
        };
        public static int maxSatellitesSent = 3;
        public static int startSpaceshipsNumber = 50;
        public static int minSpaceshipsLeft = 2;
    }

    public class ConnectedPlanets
    {
        public List<Planet> planets;

        public ConnectedPlanets()
        {
            planets = new List<Planet>();
        }

        public ConnectedPlanets(List<Planet> planets)
        {
            this.planets = planets;
        }

        private bool IsConatinedByGroup(Planet planet)
        {
            return planets.Contains(planet);
        }

        public static ConnectedPlanets GroupContainingPlanet(List<ConnectedPlanets> groups, Planet planet)
        {
            foreach (ConnectedPlanets group in groups)
            {
                if (group.IsConatinedByGroup(planet))
                    return group;
            }
            return null;
        }

        public static ConnectedPlanets MergeGroups(ConnectedPlanets group1, ConnectedPlanets group2)
        {
            return new ConnectedPlanets(group1.planets.Concat(group2.planets).ToList());
        }

        public static bool ArePlanetsInOneGroup(List<ConnectedPlanets> groups, Planet planet1, Planet planet2)
        {
            return GroupContainingPlanet(groups, planet1) == GroupContainingPlanet(groups, planet2);
        }
    }
}
