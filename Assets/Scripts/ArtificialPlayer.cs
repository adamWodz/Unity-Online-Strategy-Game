using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
        public int[,] distanceBetweenPlanets = new int[Map.mapData.planets.Count,Map.mapData.planets.Count];
        public int[,] nextPlanet = new int[Map.mapData.planets.Count, Map.mapData.planets.Count];
        public Dictionary<Planet, int> planetIds = new Dictionary<Planet, int>();

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

            UpdateDistances();
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

            DrawMissions();

            //int cardIndex = 0;
            //drawCardsPanel.AiDrawCardAndEndTurn(cardIndex, this);

            //BuildPath(Map.mapData.paths[0]);

            Debug.Log("Best Move");

            Communication.EndAITurn(this);
        }

        private void UpdateDistances()
        {
            for (int i = 0; i < Map.mapData.planets.Count; i++)
                for (int j = 0; j < Map.mapData.planets.Count; j++)
                    distanceBetweenPlanets[i, j] = int.MaxValue;

            for (int i = 0; i < Map.mapData.planets.Count; i++)
            {
                distanceBetweenPlanets[i, i] = 0;
                nextPlanet[i, i] = 0;
            }

            foreach (Path path in Map.mapData.paths)
            {
                if(path.isBuilt)
                {
                    if(path.builtById == Id)
                    {
                        distanceBetweenPlanets[planetIds[path.planetFrom], planetIds[path.planetTo]] = 0;
                        distanceBetweenPlanets[planetIds[path.planetTo], planetIds[path.planetFrom]] = 0;
                    }
                    else // path.buildById != Id
                        continue;

                }
                distanceBetweenPlanets[planetIds[path.planetFrom], planetIds[path.planetTo]] = path.length;
                distanceBetweenPlanets[planetIds[path.planetTo], planetIds[path.planetFrom]] = path.length;
                nextPlanet[planetIds[path.planetTo], planetIds[path.planetFrom]] = planetIds[path.planetFrom];
                nextPlanet[planetIds[path.planetFrom], planetIds[path.planetTo]] = planetIds[path.planetTo];
            }

            for (int k = 0; k < Map.mapData.planets.Count; k++)
                for (int i = 0; i < Map.mapData.planets.Count; i++)
                    for (int j = 0; j < Map.mapData.planets.Count; j++)
                        if (distanceBetweenPlanets[i, j] > distanceBetweenPlanets[i, k] + distanceBetweenPlanets[k, j])
                        {
                            distanceBetweenPlanets[i, j] = distanceBetweenPlanets[i, k] + distanceBetweenPlanets[k, j];
                            nextPlanet[i, j] = nextPlanet[k, j];
                        }
        }

        private void BuildPath(Path path)
        {
            distanceBetweenPlanets[planetIds[path.planetFrom], planetIds[path.planetTo]] = 0;
            distanceBetweenPlanets[planetIds[path.planetTo], planetIds[path.planetFrom]] = 0;

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

            BuildPath buildPath = Server.buildPaths.Where(b => b.path == path).First();
            buildPath.StartCoroutine(buildPath.BuildPathAnimation(Id));

            _PlayersPanel.UpdatePointsAndSpeceshipsNumServerRpc(Id, curentPoints, spaceshipsLeft);
            _GameManager.SetBuildPathDataServerRpc(path.Id, Id);

        }

        private void DrawMissions()
        {
            MissionsPanel missionsPanel = GameObject.Find("MissionsPanel").GetComponent<MissionsPanel>();
            List<Mission> bestMissions = PickBestMissions(missionsPanel.GetRandomMissions());

            foreach (Mission m in bestMissions)
            {
                missionsPanel.SyncMissionsToChooseServerRpc(m.start.name, m.end.name);
            }
        }

        // wybieranie najlepszych misji
        List<Mission> PickBestMissions(List<Mission> missions)
        {
            /*
             * dobranie pierwszych misji
             - szukamy par misji, których trasy się pokrywają
            */

            return missions;
        }

        public List<Path> QuickestWayWithLongestPaths(Mission mission)
        {
            // szukamy najkrótszej ścieżki pod względem liczby połączeń
            // jeśli dwie mają tyle samo połączeń to wybieramy to krótsze
            
            return null;
        }

        private void DrawCards()
        {
            if(BestCardColorsToDraw() == null)
                DrawRandomCard();
            else
                DrawCard();

            if (BestCardColorsToDraw() == null)
                DrawRandomCard();
            else
                DrawCard();
        }

        private void DrawCard()
        {
            List<Color> bestCardsToDraw = BestCardColorsToDraw();
            Color[] availableColors = drawCardsPanel.GetCurrentCardsToChoose();

            for(int i = 0; i < availableColors.Length; i++)
            {
                if (bestCardsToDraw.Contains(availableColors[i]))
                {
                    numOfCardsInColor[availableColors[i]]++;
                }
            }
        }

        private void DrawRandomCard()
        {

        }

        private List<Color> BestCardColorsToDraw()
        {
            return null;
        }
    }
}
