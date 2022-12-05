using Assets.GameplayControl.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameplayControl
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int curentPoints { get; set; }

        public int[] numberOfShipsInHangars;
        public int satellitesLeft;
        public List<Mission> missions;
        public Dictionary<Color, int> numOfCardsInColor = new Dictionary<Color, int>();
        bool isNowPlaying { set; get; }


        private bool CanBuildPath(Path path)
        {
            if (!isNowPlaying) return false;
            if (path.isBuilt) return false;
            if (numOfCardsInColor[path.color] < path.length) return false;

            return true;
        }

        public bool BuildPath(Path path)
        {
            if (CanBuildPath(path))
            {
                UpdatePlayerAfterBuild(path);
                return true;
            }
            return false;
        }

        public void UpdatePlayerAfterBuild(Path path)
        {
            curentPoints += path.length;
            numOfCardsInColor[path.color] -= path.length;
            path.isBuilt = true;

            PropagateChanges();
        }

        public void PropagateChanges()
        {
            // wiadomość do serwera żeby powiadomił pozostałych graczy o zmianach
            throw new NotImplementedException();
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
