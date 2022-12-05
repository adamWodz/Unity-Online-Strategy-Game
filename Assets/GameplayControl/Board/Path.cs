using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl.Board
{
    public class PathEdge
    {
        public int Id;
        public PlanetVertex planetFrom;
        public PlanetVertex planetTo;
        bool isBuilt { get; set; } = false;
        Player builtBy = null;
        bool withSatellie { get; set; } = false;
        Player playerOfSatellite = null;
    }
}
