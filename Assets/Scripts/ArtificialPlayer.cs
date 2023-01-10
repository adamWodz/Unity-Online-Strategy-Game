using JetBrains.Annotations;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace Assets.GameplayControl
{
    public class ArtificialPlayer
    {
        private DrawCardsPanel _drawCardsPanel;
        private DrawCardsPanel drawCardsPanel
        {
            get
            {
                if (_drawCardsPanel == null)
                    _drawCardsPanel = GameObject.Find("DrawCardsPanel").GetComponent<DrawCardsPanel>();
                return _drawCardsPanel;
            }
        }
        private static GameManager _gameManager;
        private static GameManager _GameManager
        {
            get
            {
                if (_gameManager == null)
                    _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                return _gameManager;
            }
        }

        private static PlayerPanel _playersPanel;
        private static PlayerPanel _PlayersPanel
        {

            get
            {
                if (_playersPanel == null)
                    _playersPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
                return _playersPanel;
            }
        }

        public int Id { set; get; }
        public string Name { set; get; }
        public int curentPoints { get; set; } = 0;
        //public int satellitesSent { get; set; } = 0;
        public int spaceshipsLeft = Board.startSpaceshipsNumber;
        public List<Mission> missions = new List<Mission>();
        public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>()
        {
            { Color.pink, 1 },
            { Color.red, 1 },
            { Color.blue, 1 },
            { Color.yellow, 1 },
            { Color.green, 1 },
            { Color.special, 1 },
        };
        public List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();
        public int[,] dist = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];
        public int[,] nextPlanet = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];
        public Dictionary<Planet, int> planetIds = new Dictionary<Planet, int>();
        public bool isLastTurn = false;

        List<Path> pathsToBuild = new List<Path>();

        public ArtificialPlayer()
        {
            int ii = 0;
            foreach (Planet planet in Map.mapData.planets)
            {
                planetIds.Add(planet, ii);
                ii++;
            }
        }

        public void BestMove()
        {
            Debug.Log("Best Move");
            SetQuickestPathForEveryPairOfPlantes();
            SetPathsToBuild();

            //PrintAllQuickestPaths();

            /*
            //BuildPath(Map.mapData.paths[0]);
            //BuildPath(Map.mapData.paths[1]);

            List<Color> colors = new List<Color>();
            colors.Add(Color.red);
            colors.Add(Color.blue);
            //DrawCard(colors);
            */

            Path path = BestPathToBuild();
            if (path != null)
            {
                Debug.Log("AI builds path");
                BuildPath(path);
            }
            else if (missions.Count > 0)
            {
                Debug.Log("AI draws cards");
                DrawCards();
                return;
            }
            else
            {
                Debug.Log("AI draws missions");
                DrawMissions();
            }

            _GameManager.EndAiTurn(this);

        }

        // sprawdzamy czy możemy wybudować jakąkiekolwiek połączenie
        Path BestPathToBuild()
        {
            pathsToBuild = pathsToBuild.OrderByDescending(p1 => p1.length).ToList();

            foreach (var path in pathsToBuild)
                if (CanBeBuild(path))
                    return path;

            return null;
        }

        bool CanBeBuild(Path path)
        {
            if (path.length > spaceshipsLeft) return false;
            if (numOfCardsInColor[path.color] < path.length
                && numOfCardsInColor[path.color] + numOfCardsInColor[Color.special] < path.length) return false;

            return true;
        }

        Dictionary<(Planet, Planet), List<Planet>> pathBetweenPlanets = new Dictionary<(Planet, Planet), List<Planet>>();
        int[,] kay = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];


        /*
        void PrintAllQuickestPaths()
        {
            foreach (Planet planet1 in Map.mapData.planets)
                foreach (Planet planet2 in Map.mapData.planets)
                {
                    Debug.Log(planet1.name + " - " + planet2.name + ": " + dist[planetIds[planet1], planetIds[planet2]]);
                    foreach (Planet planet in pathBetweenPlanets[(planet1, planet2)])
                    {
                        //Debug.Log(planet.name);
                    }
                }

            Debug.Log("paths:");
            foreach (Path path in Map.mapData.paths)
                Debug.Log(path.planetFrom + " - " + path.planetTo);
        }

        Planet uran = Map.mapData.planets.First(p => p.name == "Uran");
        Planet jowisz = Map.mapData.planets.First(p => p.name == "Jowisz");
        Planet ceres = Map.mapData.planets.First(p => p.name == "Ceres");
        */

        private void SetQuickestPathForEveryPairOfPlantes()
        {
            for (int i = 0; i < Map.mapData.planets.Count; i++)
                for (int j = 0; j < Map.mapData.planets.Count; j++)
                    kay[i, j] = -1;
            
            for (int i = 0; i < Map.mapData.planets.Count; i++)
                for (int j = 0; j < Map.mapData.planets.Count; j++)
                    dist[i, j] = int.MaxValue;

            for (int i = 0; i < Map.mapData.planets.Count; i++)
            {
                dist[i, i] = 0;
            }

            foreach (Path path in Map.mapData.paths)
            {
                if (path.isBuilt) continue;
                dist[planetIds[path.planetFrom], planetIds[path.planetTo]] = path.length;
                dist[planetIds[path.planetTo], planetIds[path.planetFrom]] = path.length;
            }

            for (int k = 0; k < Map.mapData.planets.Count; k++)
                for (int i = 0; i < Map.mapData.planets.Count; i++)
                    for (int j = 0; j < Map.mapData.planets.Count; j++)
                        if (dist[i, j] > dist[i, k] + dist[k, j] && dist[i, k] < int.MaxValue && dist[k, j] < int.MaxValue)
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            kay[i, j] = k;
                        }

            foreach(Planet planet1 in Map.mapData.planets)
                foreach(Planet planet2 in Map.mapData.planets)
                {
                    List<Planet> path = pathBetweenPlanets[(planet1, planet2)] = new List<Planet>();
                    path.Add(planet1);
                    GetPath(planetIds[planet1], planetIds[planet2], path);
                }
        }

        public void GetPath(int i, int j, List<Planet> path)
        {
            if (i == j)
                return;
            if (kay[i, j] == -1)
                path.Add(planetIds.First(p => p.Value == j).Key);
            else
            {
                GetPath(i, kay[i, j], path);
                GetPath(kay[i, j], j, path);
            }
        }

        void SetPathsToBuild()
        {
            pathsToBuild = new List<Path>();

            foreach (Mission mission in missions)
            {
                Debug.Log(mission.start.name + " " + mission.end.name);

                foreach (Path path in GetQuickestPathForMission(mission))
                    Debug.Log(path);

                pathsToBuild.AddRange(GetQuickestPathForMission(mission));
            }
        }

        List<Path> GetQuickestPathForMission(Mission mission)
        {
            List<Planet> quicketsPath;
            List<Planet> startGropup, endGroup;

            ConnectedPlanets startPlanetGroup = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, mission.start);
            if (startPlanetGroup != null)
                startGropup = startPlanetGroup.planets;
            else
            {
                startGropup = new List<Planet>();
                startGropup.Add(mission.start);
            }

            ConnectedPlanets endPlanetGroup = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, mission.end);
            if (endPlanetGroup != null)
                endGroup = endPlanetGroup.planets;
            else
            {
                endGroup = new List<Planet>();
                endGroup.Add(mission.end);
            }


            int shortestDist = dist[planetIds[mission.start], planetIds[mission.end]];
            if(shortestDist == int.MaxValue)
            {
                SetmissionAsIncompletable(mission);
                return null;
            }
            quicketsPath = pathBetweenPlanets[(mission.start, mission.end)];


            foreach(Planet planet1 in startGropup)
                foreach(Planet planet2 in endGroup)
                {
                    if (dist[planetIds[planet1], planetIds[planet2]] < shortestDist)
                    {
                        shortestDist = dist[planetIds[planet1], planetIds[planet2]];
                        quicketsPath = pathBetweenPlanets[(planet1, planet2)];
                    }
                }

            Debug.Log("Mission " + mission.name + ", length: " + quicketsPath.Count + " " + dist[planetIds[mission.start],planetIds[mission.end]]);

            return ConvertPath(quicketsPath);
        }

        List<Path> ConvertPath(List<Planet> pathOfPlanets)
        {
            List<Path> resultPath = new List<Path>();
            
            for(int i = 0; i < pathOfPlanets.Count - 1; i++)
            {
                //Debug.Log("path: " + pathOfPlanets[i] + " - " + pathOfPlanets[i + 1]);
                Path path = Map.mapData.paths.Where(p => (p.planetFrom == pathOfPlanets[i] && p.planetTo == pathOfPlanets[i + 1])
                    || (p.planetTo == pathOfPlanets[i] && p.planetFrom == pathOfPlanets[i + 1])).First();
                resultPath.Add(path);
            }

            return resultPath;
        }

        void SetmissionAsIncompletable(Mission mission)
        {
            missions.Remove(mission);
        }

        private void BuildPath(Path path)
        {
            curentPoints += Board.pointsPerLength[path.length];
            if (path.length <= numOfCardsInColor[path.color])
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

            ConnectedPlanets.AddPlanetsFromPathToPlanetsGrups(path, groupsOfConnectedPlanets);

            BuildPath buildPath = Server.buildPaths.Where(b => b.path == path).First();
            //buildPath.StartCoroutine(buildPath.BuildPathAnimation(Id));
            buildPath.DoBuildPathByAI(Id);

            _PlayersPanel.UpdatePointsAndSpeceshipsNumServerRpc(Id, curentPoints, spaceshipsLeft);
            _GameManager.SetBuildPathDataServerRpc(path.Id, Id);

        }

        private void DrawMissions()
        {
            MissionsPanel missionsPanel = GameObject.Find("MissionsPanel").GetComponent<MissionsPanel>();
            List<Mission> bestMissions = PickBestMissions(missionsPanel.GetRandomMissions());
            missions.AddRange(bestMissions);

            foreach (Mission m in bestMissions)
            {
                missionsPanel.SyncMissionsToChooseServerRpc(m.start.name, m.end.name);
            }
        }

        // wybieranie najlepszych misji
        List<Mission> PickBestMissions(List<Mission> missionsToDraw)
        {
            List<Mission> pickedMissions = new List<Mission>();
            List<Mission> missionsPool = missionsToDraw;
            missionsPool.AddRange(missions);
            
            foreach(Mission mission1 in missionsPool)
            {
                foreach(Mission mission2 in missionsPool)
                {
                    if (mission1 == mission2)
                        continue;
                    if (mission1.start == mission2.start || mission1.start == mission2.end ||
                        mission1.end == mission2.start || mission1.end == mission2.end)
                    {
                        if (!missions.Contains(mission1) && !pickedMissions.Contains(mission1))
                            pickedMissions.Add(mission1);
                        if (!missions.Contains(mission2) && !pickedMissions.Contains(mission1))
                            pickedMissions.Add(mission2);
                    }
                }
            }

            while (pickedMissions.Count > 3)
                pickedMissions.RemoveAt(0);

            if (pickedMissions.Count == 0)
                pickedMissions.Add(missionsToDraw.First());

            return pickedMissions;
        }

        private void DrawCards()
        {
            Debug.Log("start drawing cards");
            
            var colors = BestCardColorsToDraw();
            
            if (!DrawCard(colors))
                DrawRandomCard();

            if (!DrawCard(colors))
                DrawRandomCard();

            Debug.Log("end drawing cards");
        }

        private bool DrawCard(List<Color> colors)
        {
            Color[] availableColors = drawCardsPanel.GetCurrentCardsToChoose();

            for(int i = 0; i < availableColors.Length; i++)
            {
                if (colors.Contains(availableColors[i]))
                {
                    Debug.Log(availableColors[i]);
                    
                    numOfCardsInColor[availableColors[i]]++;
                    drawCardsPanel.AiDrawCard(i, this);
                    return true;
                }
            }

            return false;
        }

        private void DrawRandomCard()
        {
            Color[] availableColors = drawCardsPanel.GetCurrentCardsToChoose();
            numOfCardsInColor[availableColors.Last()]++;

            Debug.Log(availableColors.Last());

            //drawCardsPanel.AiDrawCard(5, this);
        }

        private List<Color> BestCardColorsToDraw()
        {
            List<Color> colors = new List<Color>();

            foreach (var path in pathsToBuild)
                if (!colors.Contains(path.color))
                    colors.Add(path.color);

            return colors;
        }
    }
}
