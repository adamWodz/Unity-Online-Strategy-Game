using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameplayControl
{
    public static class PlayerGameData
    {
        public static int Id { get; }
        public static string Name { get; }
        public static int curentPoints { get; set; } = 0;
        public static int spaceshipsLeft { get; set; } = Board.startSpaceshipsNumber;
        public static int satellitesSent { get; set; } = 0;
        public static List<Mission> missions;
        public static Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>()
        {
            { Color.pink, 1 },
            { Color.red, 1 },
            { Color.blue, 1 },
            { Color.yellow, 1 },
            { Color.green, 1 },
            { Color.special, 1 },
        };
        public static bool isNowPlaying { set; get; }
        public static int cardsDrewInTurn = 0;

        public static List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();

        public static bool CanBuildPath(Path path)
        {
            //if (!isNowPlaying) return false;
            if (path.isBuilt) return false;
            if (numOfCardsInColor[path.color] < path.length) return false;

            return true;
        }

        public static bool BuildPath(Path path)
        {
            if (!CanBuildPath(path)) return false;
            
            curentPoints += Board.pointsPerLength[path.length];
            numOfCardsInColor[path.color] -= path.length;
            spaceshipsLeft -= path.length;
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

        public static bool CanSendSatellite(Planet planet, Path path, Color color)
        {
            if (planet.withSatellite) return false;
            if (path.withSatellie) return false;
            if (satellitesSent >= Board.maxSatellitesSent) return false;
            if (numOfCardsInColor[color] < Board.cardsPerSatelliteSend[satellitesSent + 1]) return false;
            
            return true;
        }

        public static bool SendSatellite(Planet planet, Path path, Color color)
        {
            if(!CanSendSatellite(planet, path, color)) return false;


            satellitesSent++;
            planet.withSatellite = true;
            return true;
        }

        public static void DrawCards(Color firstCardsColor, Color secondCardColor)
        {
            numOfCardsInColor[firstCardsColor]++;
            numOfCardsInColor[secondCardColor]++;
            cardsDrewInTurn += 2;
        }

        public static void DrawCard(Color cardColor)
        {
            numOfCardsInColor[cardColor]++;
            cardsDrewInTurn++;
        }

        public static void StartTurn()
        {
            isNowPlaying = true;
        }

        public static void EndTurn()
        {
            isNowPlaying = false;
            cardsDrewInTurn = 0;
        }

        public static void SetPathIsBuild(int pathId)
        {
            Path builtPath = GameObject.Find("Space").GetComponent<MapData>().paths.Where(path => path.Id == pathId).FirstOrDefault();
            builtPath.isBuilt = true;
        }
    }
}
