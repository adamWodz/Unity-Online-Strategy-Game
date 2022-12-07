using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl
{
    public class Player
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public int curentPoints { get; set; } = 0;
        public int satellitesSent { get; set; } = 0;
        public List<Mission> missions;
        public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>()
        {
            { Color.pink, 0 },
            { Color.red, 0 },
            { Color.black, 0 },
            { Color.blue, 0 },
            { Color.white, 0 },
            { Color.yellow, 0 },
            { Color.green, 0 },
            { Color.special, 0 },
        };
        bool isNowPlaying { set; get; }

        public List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();

        public Player(string name, List<Mission> missions)
        {
            Name = name;
            this.missions = missions;
        }

        protected bool CanBuildPath(Path path)
        {
            if (!isNowPlaying) return false;
            if (path.isBuilt) return false;
            if (numOfCardsInColor[path.color] < path.length) return false;

            return true;
        }

        public bool BuildPath(Path path)
        {
            if (!CanBuildPath(path)) return false;
            
            curentPoints += Board.pointsPerLength[path.length];
            numOfCardsInColor[path.color] -= path.length;
            path.isBuilt = true;

            // wiadomość do serwera żeby powiadomił pozostałych graczy o zmianach
            // to do

            // dodanie planet do grup połączonych planet
            ConnectedPlanets groupPlanetFrom = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, path.planetFrom);
            ConnectedPlanets groupPlanetTo = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, path.planetTo);

            // jeśli obu planet nie ma w żadnej grupie, to tworzymy nową grupę
            if (groupPlanetFrom == null && groupPlanetTo == null)
            {
                groupsOfConnectedPlanets.Add(new ConnectedPlanets(new List<Planet> { path.planetFrom, path.planetTo }));
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
                groupsOfConnectedPlanets.Remove(groupPlanetTo);
                groupsOfConnectedPlanets.Remove(groupPlanetFrom);
                groupsOfConnectedPlanets.Add(ConnectedPlanets.MergeGroups(groupPlanetTo, groupPlanetFrom));
            }

            return true;
        }

        protected bool CanSendSatellite(Planet planet, Path path, Color color)
        {
            if (planet.withSatellite) return false;
            if (path.withSatellie) return false;
            if (satellitesSent >= Board.maSatellitesSent) return false;
            if (numOfCardsInColor[color] < Board.cardsPerSatelliteSend[satellitesSent + 1]) return false;
            
            return true;
        }

        public bool SendSatellite(Planet planet, Path path, Color color)
        {
            if(!CanSendSatellite(planet, path, color)) return false;


            satellitesSent++;
            planet.withSatellite = true;
            return true;
        }

        public void DrawCards(Color firstCardsColor, Color secondCardColor)
        {
            numOfCardsInColor[firstCardsColor]++;
            numOfCardsInColor[secondCardColor]++;
        }

        public void NewTurn()
        {
            isNowPlaying = true;
        }

        public void EndTurn()
        {
            isNowPlaying = false;
            // wiadomość do serwera o zakończonej turze
            throw new NotImplementedException();
        }
    }
}
