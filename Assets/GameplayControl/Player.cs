using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine;
namespace Assets.GameplayControl
{
    public static class PlayerGameData
    {
        private static MapData _mapData;
        private static MapData mapData
        {
            get
            {
                if (_mapData == null)
                    _mapData = GameObject.Find("Space").GetComponent<MapData>();
                return _mapData;
            }
        }

        public static int Id { set;  get; }
        public static string Name { set; get; } = "Gracz";

        public static string UnityId { set; get; }

        public static List<string> AINames = new List<string> 
        {
            "Hans Olo",
            "Jabba",
            "Shepard",
            "Atryda"
        };

        public static int curentPoints { get; set; } = 0;
        public static int spaceshipsLeft { get; set; } = Board.startSpaceshipsNumber;
        public static int satellitesSent { get; set; } = 0;
        public static bool lastTurn = false;
        public static List<Mission> missions = new List<Mission>();
        public static List<Mission> completedMissions = new List<Mission>();
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
        public static bool isDrawingMission = false;
        public static int cardsDrewInTurn = 0;

        public static List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();

        public static bool CanBuildPath(Path path, out string errorMessage)
        {
            if (!isNowPlaying)
            {
                errorMessage = "Brak obecnie ruchu.";
                return false;
            }
            if (isDrawingMission)
            {
                errorMessage = "Nie możesz teraz wybudować połączenia.";
                return false;
            }
            if (cardsDrewInTurn > 0)
            {
                errorMessage = "W tym ruchu juz dobrano kartę.";
                return false;
            }
            if (path.isBuilt)
            {
                errorMessage = "Połączenie jest juz wybudowane.";
                return false;
            }
            if (path.length > spaceshipsLeft)
            {
                errorMessage = "Za mało statków.";
                return false;
            }
            if (numOfCardsInColor[path.color] < path.length
                && numOfCardsInColor[path.color] + numOfCardsInColor[Color.special] < path.length)
            {
                errorMessage = "Za mało kart w odpowiednim kolorze.";
                return false;
            }

            errorMessage = "No error";
            return true;
        }

        public static bool BuildPath(Path path)
        {
            curentPoints += Board.pointsPerLength[path.length];
            if(path.length <= numOfCardsInColor[path.color])
            {
                numOfCardsInColor[path.color] -= path.length;
            }
            else
            {
                int pathLenLeft = path.length - numOfCardsInColor[path.color];
                numOfCardsInColor[path.color] = 0;
                numOfCardsInColor[Color.special] -= pathLenLeft;
            }
            spaceshipsLeft -= path.length;
            path.isBuilt = true;

            ConnectedPlanets.AddPlanetsFromPathToPlanetsGroups(path, groupsOfConnectedPlanets);

            PrintConnectedPlanets();
            return true;
        }

        public static void PrintConnectedPlanets()
        {
            Debug.Log("Connected planets: ");
            foreach (var planets in groupsOfConnectedPlanets)
            {
                Debug.Log("group:");
                foreach (var planet in planets.planets)
                    Debug.Log(planet);
            }
        }

        public static void PrintMissions()
        {
            Debug.Log("Missions:");
            foreach (var mission in missions)
                Debug.Log(mission + "; " + mission.IsCompletedByClientPlayer());
        }

        public static List<Mission> GetNewCompletedMissions()
        {
            List<Mission> newCompletedMissions = new List<Mission>();

            foreach(var mission in missions)
            {
                if(mission.IsCompletedByClientPlayer())
                    if(!completedMissions.Contains(mission))
                    {
                        completedMissions.Add(mission);
                        newCompletedMissions.Add(mission);
                        mission.isDone = true;
                    }
            }

            return newCompletedMissions;
        }

        public static void PrintCards()
        {
            int n = numOfCardsInColor.Count;
            for (int i = 0; i < n; i++)
                Debug.Log(i + ". color: " + (Color)i + "; " + numOfCardsInColor[(Color)i]);
        }

        public static bool CanSendSatellite(Planet planet, Path path, Color color)
        {
            if (planet.withSatellite) return false;
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
            DrawCard(firstCardsColor, true);
            DrawCard(secondCardColor, true);
        }

        public static bool CanDrawCard(out string errorMessage)
        {
            if(isDrawingMission)
            {
                errorMessage = "Nie możesz dobrać karty gdy wybierasz misje.";
                return false;
            }

            errorMessage = "No error";
            return true;
        }

        public static void DrawCard(Color cardColor, bool randomDraw)
        {
            numOfCardsInColor[cardColor]++;
            cardsDrewInTurn++;
            if (cardColor == Color.special && !randomDraw)
                cardsDrewInTurn++;
        }

        public static bool CanDrawMission(out string errorMessage)
        {
            if (!isNowPlaying)
            {
                errorMessage = "Brak obecnie ruchu.";
                return false;
            }
            if (cardsDrewInTurn > 0)
            {
                errorMessage = "W tym ruchu juz dobrano kartę.";
                return false;
            }

            errorMessage = "No error";
            return true;
        }

        public static void DrawMissions(List<Mission> _missions)
        {
            missions.AddRange(_missions.Except(missions));
        }

        public static void StartTurn()
        {
            isNowPlaying = true;
            cardsDrewInTurn = 0;
        }

        public static void EndTurn()
        {
            isNowPlaying = false;
        }

        public static void SetPathIsBuild(int pathId)
        {
            Path builtPath = mapData.paths.Where(path => path.Id == pathId).FirstOrDefault();
            builtPath.isBuilt = true;
        }

        public static void CalculateFinalPoints()
        {
            foreach (Mission mission in missions)
                if (mission.isDone)
                    curentPoints += mission.points;
                else
                    curentPoints -= mission.points;
        }

        public static int[] GetMissionIds()
        {
            int[] ids = new int[missions.Count];

            for (int i = 0; i < missions.Count; i++)
                ids[i] = missions[i].id;

            return ids;
        }

        public static bool[] AreMissionsDone()
        {
            bool[] areDone = new bool[missions.Count];

            for (int i = 0; i < missions.Count; i++)
                areDone[i] = missions[i].isDone;

            return areDone;
        }

        public static void Reset()
        {
            curentPoints = 0;
            spaceshipsLeft = Board.startSpaceshipsNumber;
            satellitesSent = 0;
            lastTurn = false;
            missions = new List<Mission>();
            completedMissions = new List<Mission>();
            numOfCardsInColor = new Dictionary<Color, int>()
            {
                { Color.pink, 1 },
                { Color.red, 1 },
                { Color.blue, 1 },
                { Color.yellow, 1 },
                { Color.green, 1 },
                { Color.special, 1 },
            };
            isNowPlaying = false;
            isDrawingMission = false;
            cardsDrewInTurn = 0;
            groupsOfConnectedPlanets = new List<ConnectedPlanets>();
    }
    }
}
