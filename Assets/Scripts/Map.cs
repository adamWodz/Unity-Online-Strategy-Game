using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using TMPro;

public class Map : MonoBehaviour
{
    public static MapData mapData;

    private float mapZparam = -1;
    private List<Path> paths;
    public List<Path> Paths
    {
        get { return paths; }
    }
    private List<Mission> missions;
    public List<Mission> Missions 
    { 
        get { return missions; }
        set { missions = value; }
    }
    private List<Planet> planets;
    public GameObject[] pathsPrefabs;
    public GameObject[] planetsPrefabs;
    public GameObject planetNameText;

    // kolory sciezek
    private UnityEngine.Color[] colors = new UnityEngine.Color[] {
        UnityEngine.Color.red,
        UnityEngine.Color.green,
        UnityEngine.Color.blue,
        UnityEngine.Color.yellow,
        UnityEngine.Color.magenta,
        UnityEngine.Color.grey,
    };

    // canvas, na którym będą wyświetlane nazwy planet na planszy (world space)
    private Canvas canvasForPlanetsNames;

    // Start is called before the first frame update
    void Start()
    {
        canvasForPlanetsNames = GameObject.Find("CanvasForPlanetsNames").GetComponent<Canvas>();
        Debug.Log(mapData);
        paths = mapData.paths;
        planets = mapData.planets; 
        missions = mapData.missions;
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateMap()
    {
        // Tworzenie planet
        for (int i = 0; i < planets.Count; i++)
        {
            // spawneine planety
            var planet = Instantiate(planetsPrefabs.Single(planet => planet.name.StartsWith(planets[i].name)), new Vector3(planets[i].positionX, planets[i].positionY, mapZparam), planetsPrefabs[i].transform.rotation);
            planet.name = planets[i].name;

            // spawnienie tekstu planety, znajdującego się nad nią, który na razie jest nieaktywny
            var planetNameText = Instantiate(this.planetNameText, new Vector3(planets[i].positionX, planets[i].positionY + 0.5f, mapZparam), Quaternion.identity);
            planetNameText.GetComponent<TMP_Text>().text = planets[i].name;
            planetNameText.name = planets[i].name + "Text";
            planetNameText.transform.SetParent(canvasForPlanetsNames.transform);
            //planetNameText.SetActive(false);
        }

        // Tworzenie ?cie?ek
        for (int i = 0; i < paths.Count; i++)
        {
            // k?t nachylenia mi?dzy dwoma planetami
            float angle = Mathf.Atan2(
                          paths[i].planetTo.positionX - paths[i].planetFrom.positionX,
                          paths[i].planetTo.positionY - paths[i].planetFrom.positionY
                          ) * Mathf.Rad2Deg;

            // ?rodkowy punkt pomi?dzy planetami
            Vector2 position = Vector2.Lerp(new Vector2(paths[i].planetTo.positionX, paths[i].planetTo.positionY), 
                new Vector2(paths[i].planetFrom.positionX, paths[i].planetFrom.positionY), 0.5f);

            Debug.Log(paths[i].length - 1);
            var pathGameObject = Instantiate(pathsPrefabs[paths[i].length - 1], position, Quaternion.Euler(new Vector3(0, 0, -angle)));

            // przypisanie do build path
            var buildPath = pathGameObject.GetComponent<BuildPath>();
            buildPath.path = paths[i];

            var tilesRenderers = pathGameObject.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < tilesRenderers.Length; j++)
            {
                tilesRenderers[j].material.color = colors[(int)paths[i].color];
            }
        }
        
    }
}