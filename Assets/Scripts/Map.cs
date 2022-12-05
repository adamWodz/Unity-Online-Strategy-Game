using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<Path> paths;
    public List<Planet> planets;
    public GameObject[] pathsPrefabs;
    public GameObject[] planetsPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateMap()
    {
        // Tworzenie planet
        for(int i=0;i<planets.Count;i++) 
        {
            Instantiate(planetsPrefabs[0], planets[i].position, planetsPrefabs[0].transform.rotation);
        }

        // Tworzenie œcie¿ek
        for(int i=0;i< paths.Count;i++)
        {
            // k¹t nachylenia miêdzy dwoma planetami
            float angle = Mathf.Atan2(
                          planets[paths[i].planetsIds[1]].position.x - planets[paths[i].planetsIds[0]].position.x, 
                          planets[paths[i].planetsIds[1]].position.y - planets[paths[i].planetsIds[0]].position.y
                          ) * Mathf.Rad2Deg; 
            Debug.Log($"Planet{paths[i].planetsIds[0]} position: {planets[paths[i].planetsIds[0]].position}");
            Debug.Log($"Planet{paths[i].planetsIds[1]} position: {planets[paths[i].planetsIds[1]].position}");
            Debug.Log($"Planets {paths[i].planetsIds[0]} - {paths[i].planetsIds[1]} angle: {angle}");
            
            // œrodkowy punkt pomiêdzy planetami
            Vector2 position = Vector2.Lerp(planets[paths[i].planetsIds[0]].position, planets[paths[i].planetsIds[1]].position, 0.5f); 
            
            Instantiate(pathsPrefabs[paths[i].length - 1], position, Quaternion.Euler(new Vector3(0,0,-angle)));
        }
    }
}
