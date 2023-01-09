using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                if(_playersPanel == null)
                    _playersPanel = GameObject.Find("PlayersPanel").GetComponent<PlayerPanel>();
                return _playersPanel;
            }
        }

        public int Id { set;  get; }
        public string Name { set; get; }
        public int curentPoints { get; set; } = 0;
        //public int satellitesSent { get; set; } = 0;
        public int spaceshipsLeft = Board.startSpaceshipsNumber;
        public List<Mission> missions;
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
        public int[,] dist = new int[Map.mapData.planets.Count,Map.mapData.planets.Count];
        public int[,] nextPlanet = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];
        public Dictionary<Planet, int> planetIds = new Dictionary<Planet, int>();
        
        List<Path> pathsToBuild = new List<Path>();

        // potrzebne struktutry:
        // - kolejka priorytetowa - brakujące kolory
        // - do rozszerzonego alg Dijkstry
        // - słownik do odległości z każdego wierzchołka do każdego i kolejnego ruchu

        public ArtificialPlayer()
        {
            int ii = 0;
            foreach(Planet planet in Map.mapData.planets)
            {
                planetIds.Add(planet, ii);
                ii++;
            }

            //UpdateDistancesAndNextMoves();
        }

        public void BestMove()
        {
            /*
             * na początku:
            - znalezienie najkrótszych tras do zrealizowania misji
            - wybranie tych z najdłuższymi połączeniami
            - jeśli nie starczy kart do wybudowania zadnej z nich - dobranie kart
            
             * co turę
            - jeśli któryś z graczy zabudował aktualna najlepsza trasę - wybranie nowej
            - jesli nowa najlepsza jest dłuższa od x, to odpuszczamy ją
            - znalezienie połączenia, które możemy wybudować
            - kolory kart, których brakuje, posortowane najpierw po tym ile brakuje, potem po długości ich połączeń
            - jeśli nie możemy nic wybudować - dobieramy karty, priorytetem jest posortowanie
            - jeśli do spełnienia wszyskich misji brakuje 3 połączeń, dobranie nowej misji
            - szukamy najkrótszej trasy sposród dostępnych
            - przy czym połączona miasta sąsiadują ze sobą
             */

            //DrawMissions();

            

            //BuildPath(Map.mapData.paths[0]);

            SetQuickestPathForEveryPairOfPlantes();
            SetPathsToBuild();

            Path path = BestPathToBuild();
            if (path != null)
                BuildPath(path);
            else if (missions.Count > 0)
                DrawCards();
            else
                DrawMissions();


            Debug.Log("Best Move");

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

        /*
        private void UpdateDistancesAndNextMoves()
        {
            for (int i = 0; i < Map.mapData.planets.Count; i++)
                for (int j = 0; j < Map.mapData.planets.Count; j++)
                    dist[i, j] = int.MaxValue;

            for (int i = 0; i < Map.mapData.planets.Count; i++)
            {
                dist[i, i] = 0;
                nextPlanet[i, i] = 0;
            }

            foreach (Path path in Map.mapData.paths)
            {
                if(path.isBuilt)
                {
                    if(path.builtById == Id)
                    {
                        dist[planetIds[path.planetFrom], planetIds[path.planetTo]] = 0;
                        dist[planetIds[path.planetTo], planetIds[path.planetFrom]] = 0;
                    }
                    else //if path.buildById != Id
                        continue;

                }
                dist[planetIds[path.planetFrom], planetIds[path.planetTo]] = path.length;
                dist[planetIds[path.planetTo], planetIds[path.planetFrom]] = path.length;
                nextPlanet[planetIds[path.planetTo], planetIds[path.planetFrom]] = planetIds[path.planetFrom];
                nextPlanet[planetIds[path.planetFrom], planetIds[path.planetTo]] = planetIds[path.planetTo];
            }

            for (int k = 0; k < Map.mapData.planets.Count; k++)
                for (int i = 0; i < Map.mapData.planets.Count; i++)
                    for (int j = 0; j < Map.mapData.planets.Count; j++)
                        if (dist[i, j] > dist[i, k] + dist[k, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            nextPlanet[i, j] = nextPlanet[i, k];
                        }
        }
        */

        /*
        void SetPathsToBuild()
        {
            pathsToBuild = new List<Path>();

            foreach (Mission mission in missions)
            {
                if (dist[planetIds[mission.start], planetIds[mission.end]] == int.MaxValue)
                    continue;
                int i = planetIds[mission.start];
                int end = planetIds[mission.end];
                while (i != end)
                {
                    int next = nextPlanet[i, end];
                    Planet planet1 = planetIds.First(p => p.Value == i).Key;
                    Planet planet2 = planetIds.First(p => p.Value == next).Key;
                    Path path = Map.mapData.paths.Where(p => (p.planetTo == planet1 && p.planetFrom == planet2) ||
                        (p.planetTo == planet2 && p.planetFrom == planet1)).First();
                    pathsToBuild.Add(path);
                    i = next;
                }
            }
        }
        */

        Dictionary<(Planet, Planet), List<Planet>> pathBetweenPlanets = new Dictionary<(Planet, Planet), List<Planet>>();
        int[,] kay = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];

        private void SetQuickestPathForEveryPairOfPlantes()
        {
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
            if (kay[i, j] == 0)
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
                pathsToBuild.AddRange(GetQuickestPathForMission(mission));
        }

        List<Path> GetQuickestPathForMission(Mission mission)
        {
            List<Planet> quicketsPath;

            ConnectedPlanets startPlanetGroup = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, mission.start);
            ConnectedPlanets endPlanetGroup = ConnectedPlanets.GroupContainingPlanet(groupsOfConnectedPlanets, mission.end);

            int shortestDist = dist[planetIds[mission.start], planetIds[mission.end]];
            if(shortestDist == int.MaxValue)
            {
                SetmissionAsIncompletable(mission);
                return null;
            }
            quicketsPath = pathBetweenPlanets[(mission.start, mission.end)];

            foreach(Planet planet1 in startPlanetGroup.planets)
                foreach(Planet planet2 in endPlanetGroup.planets)
                {
                    if (dist[planetIds[planet1], planetIds[planet2]] < shortestDist)
                    {
                        shortestDist = dist[planetIds[planet1], planetIds[planet2]];
                        quicketsPath = pathBetweenPlanets[(planet1, planet2)];
                    }
                }

            return ConvertPath(quicketsPath);
        }

        List<Path> ConvertPath(List<Planet> path)
        {
            // to do
            
            return null;
        }

        void SetmissionAsIncompletable(Mission mission)
        {
            missions.Remove(mission);
        }

        private void BuildPath(Path path)
        {
            //dist[planetIds[path.planetFrom], planetIds[path.planetTo]] = 0;
            //dist[planetIds[path.planetTo], planetIds[path.planetFrom]] = 0;

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
            buildPath.StartCoroutine(buildPath.BuildPathAnimation(Id));

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
        List<Mission> PickBestMissions(List<Mission> missions)
        {
            // to do
            
            /*
             * dobranie pierwszych misji
             - szukamy par misji, których trasy się pokrywają
            */

            return missions;
        }

        /*
        public List<Path> QuickestWayWithLongestPaths(Mission mission)
        {
            // szukamy najkrótszej ścieżki pod względem liczby połączeń
            // jeśli dwie mają tyle samo połączeń to wybieramy to krótsze
            
            return null;
        }
        */

        private void DrawCards()
        {
            var colors = BestCardColorsToDraw();
            
            if (!DrawCard(colors))
                DrawRandomCard();

            if (!DrawCard(colors))
                DrawRandomCard();
        }

        private bool DrawCard(List<Color> colors)
        {
            Color[] availableColors = drawCardsPanel.GetCurrentCardsToChoose();

            for(int i = 0; i < availableColors.Length; i++)
            {
                if (colors.Contains(availableColors[i]))
                {
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
            drawCardsPanel.AiDrawCard(5, this);
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
