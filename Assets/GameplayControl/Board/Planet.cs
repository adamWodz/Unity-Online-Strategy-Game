using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl.Board
{
    public class PlanetVertex
    {
        public int Id;
        public string Name;
        public List<PathEdge> adjacentPaths;
    }

    public class ConnectedPlanets
    {
        List<PlanetVertex> planets;

        private bool IsConatinedByGroup(PlanetVertex planet)
        {
            return planets.Contains(planet);
        }

        public static bool IsPlanetContainedByAnyGroup(PlanetVertex planet, List<ConnectedPlanets> groups)
        {
            foreach(ConnectedPlanets group in groups)
            {
                if(group.IsConatinedByGroup(planet))
                    return true;
            }
            return false;
            // ile
        }

        public static void MergeGroups(List<ConnectedPlanets> groups, ConnectedPlanets group1, ConnectedPlanets group2)
        {
            ConnectedPlanets newGroup = new();
            // łączenie różnych ilości
        }
    }
}
