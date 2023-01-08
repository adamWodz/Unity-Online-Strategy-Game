using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        public int Id { set;  get; }
        public string Name { set; get; }
        public int curentPoints { get; set; } = 0;
        //public int satellitesSent { get; set; } = 0;
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
        public static List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();

        // potrzebne struktutry:
        // - kolejka priorytetowa - brakujące kolory
        // - do rozszerzonego alg Dijkstry
        // - słownik do odległości z każdego wierzchołka do każdego i kolejnego ruchu

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

            Debug.Log("Best Move");

            Communication.EndAITurn();
        }

        private void UpdateBestOptions()
        {

        }

        // wybieranie najlepszych misji
        void PickBestMissions()
        {
            /*
             * dobranie pierwszych misji
             - szukamy par misji, których trasy się pokrywają
            */
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
