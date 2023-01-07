using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeePlanetName : MonoBehaviour
{

    GameObject planetNameText;

    // Start is called before the first frame update
    void Start()
    {
        planetNameText = GameObject.Find(transform.name+"Text");
        planetNameText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if(planetNameText.activeSelf) 
        {
            planetNameText.SetActive(false);
        }
        else
        {
            planetNameText.SetActive(true);
        }
    }
}
