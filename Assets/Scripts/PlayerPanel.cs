using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerPanel : MonoBehaviour
{
    [Serializable]
    public struct PlayerInfo
    {
        public int Position;
        public int Points;
        public string Name;
    }
    
    [SerializeField] public List<PlayerInfo> players;

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerTextTempalte = transform.GetChild(0).gameObject;
        GameObject pom;

        int n = players.Count;

        for (int i = 0; i < n; i++)
        {
            pom = Instantiate(playerTextTempalte, transform);
            pom.transform.GetChild(0).GetComponent<TMP_Text>().text = players[i].Position.ToString();
            pom.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
        }
        Destroy(playerTextTempalte);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
