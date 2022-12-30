using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl
{
    public class ArtificialPlayer
    {
        public int Id { get; }
        public string Name { get; }
        public int curentPoints { get; set; } = 0;
        public int satellitesSent { get; set; } = 0;
        public List<Mission> missions;
        public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>()
        {
            { Color.pink, 0 },
            { Color.red, 0 },
            //{ Color.black, 0 },
            { Color.blue, 0 },
            //{ Color.white, 0 },
            { Color.yellow, 0 },
            { Color.green, 0 },
            { Color.special, 0 },
        };
        public static List<ConnectedPlanets> groupsOfConnectedPlanets = new List<ConnectedPlanets>();

        // potrzebne struktutry:
        // - kolejka priorytetowa - brakujące kolory
        // - do rozszerzonego alg Dijkstry
        // - słownik do odległości z każdego wierzchołka do każdego i kolejnego ruchu

        // wybieranie najlepszych misji
        void PickBestMissions()
        {
            /*
             * dobranie pierwszych misji
             - szukamy par misji, których trasy się pokrywają
             
             */
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
        }

        public List<Path> QuickestPath()
        {
            return null;
        }
    }
}
