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
            {7, 22},
            {8, 29}
        };
        public static Dictionary<int, int> cardsPerSatelliteSend = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 3},
            {3, 3},
        };
        public static int maxSatellitesSent = 3;
        public static int startSpaceshipsNumber = 15;
        public static int minSpaceshipsLeft = 3;
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
            ConnectedPlanets groupOfPlanet1 = GroupContainingPlanet(groups, planet1);
            ConnectedPlanets groupOfPlanet2 = GroupContainingPlanet(groups, planet2);

            if (groupOfPlanet1 == null && groupOfPlanet2 == null) return false;

            return GroupContainingPlanet(groups, planet1) == GroupContainingPlanet(groups, planet2);
        }

        public static void AddPlanetsFromPathToPlanetsGroups(Path path, List<ConnectedPlanets> connectedPlanets)
        {
            // najpierw sprawdzamy czy planety są już w jakichs grupach
            ConnectedPlanets groupPlanetFrom = ConnectedPlanets.GroupContainingPlanet(connectedPlanets, path.planetFrom);
            ConnectedPlanets groupPlanetTo = ConnectedPlanets.GroupContainingPlanet(connectedPlanets, path.planetTo);

            // jeśli obu planet nie ma w żadnej grupie, to tworzymy nową grupę
            if (groupPlanetFrom == null && groupPlanetTo == null)
            {
                connectedPlanets.Add(new ConnectedPlanets(new List<Planet> { path.planetFrom, path.planetTo }));
            }
            // dodajemy nowo połączoną planetę do grupy
            else if (groupPlanetFrom == null)
            {
                groupPlanetTo.planets.Add(path.planetFrom);
            }
            else if (groupPlanetTo == null)
            {
                groupPlanetFrom.planets.Add(path.planetTo);
            }
            // jesli obie planety należą do innej grupy, to łączymy te grupy
            else if (groupPlanetFrom != groupPlanetTo)
            {
                connectedPlanets.Remove(groupPlanetTo);
                connectedPlanets.Remove(groupPlanetFrom);
                connectedPlanets.Add(ConnectedPlanets.MergeGroups(groupPlanetTo, groupPlanetFrom));
            }
        }
    }
}
