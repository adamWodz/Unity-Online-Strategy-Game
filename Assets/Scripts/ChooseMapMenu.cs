using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseMapMenu : NetworkBehaviour
{
    public void ChooseMap0()
    {
        Communication.mapDataNumber = 0;
    }

    public void ChooseMap1()
    {
        Communication.mapDataNumber = 1;
    }

    public void ChooseMap2()
    {
        Communication.mapDataNumber = 2;
    }

    public void ChooseMap3()
    {
        Communication.mapDataNumber = 3;
    }
}
