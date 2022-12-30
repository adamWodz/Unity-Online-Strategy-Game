using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class JoinMenuInputField : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return)) 
        {
            // logika lobby

            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsClient && !networkManager.IsServer)
            {
                networkManager.StartClient();
            }
        }
    }
}
