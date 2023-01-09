using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
   public void LoadGame()
    {
        Communication.loadOnStart = true;
    }
}
