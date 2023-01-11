using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using TMPro;

public class Map : NetworkBehaviour, IDataPersistence
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
    public List<Planet> Planets { get { return planets; } }
    public GameObject[] pathsPrefabs;
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

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (!Communication.loadOnStart)
        {
            canvasForPlanetsNames = GameObject.Find("CanvasForPlanetsNames").GetComponent<Canvas>();
            //Debug.Log(canvasForPlanetsNames);
            //Debug.Log("Host?" + IsHost);
            //Debug.Log("Client?" + IsClient);
            //Debug.Log("Server?" + IsServer);
            //Debug.Log(mapData);
            paths = mapData.paths;
            planets = mapData.planets;
            missions = mapData.missions;
            CreateMap();
        }
        else
            paths = new();
        
    }

    public void LoadData(GameData data)
    {
        //Debug.Log("Host?"+IsHost);
        //Debug.Log("Client?" + IsClient);
        //Debug.Log("Server?" + IsServer);
        if (IsHost)
        {
            //paths = data.paths;
            //Communication.mapDataNumber = data.mapNumber;
            foreach(PathData path in data.paths)
            {
                SetPathsClientRpc(path.id, path.planetFromName, path.planetToName, path.color, path.length, path.isBuilt, path.builtById,data.mapNumber);
            }
            SetMapDataClientRpc();
        }

    }

    public void SaveData(ref GameData data)
    {
        if (IsHost)
        {
            // Debug.Log("Host?" + IsHost);
            //Debug.Log("Client?" + IsClient);
            //Debug.Log("Server?" + IsServer);
            //data.paths = paths;
            data.paths ??= new();
            foreach(Path path in paths)
            {
                PathData pathData = new()
                {
                    id = path.Id,
                    planetFromName = path.planetFrom.name,
                    planetToName= path.planetTo.name,
                    color = path.color,
                    length = path.length,
                    isBuilt = path.isBuilt,
                    builtById = path.builtById,
                };
                data.paths.Add(pathData);
            }
            data.mapNumber = Communication.mapDataNumber;
        }
    }

    void CreateMap()
    {
        // Tworzenie planet
        for (int i = 0; i < planets.Count; i++)
        {
            // spawneine planety
            var planet = Instantiate(planets[i].planetPrefab, new Vector3(planets[i].positionX, planets[i].positionY, mapZparam), planets[i].planetPrefab.transform.rotation);
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
            Debug.Log($"Czy path {paths[i].planetFrom.name}-{paths[i].planetTo.name} jest zbudowana? {paths[i].isBuilt}");
            Server.buildPaths.Add(buildPath);

            var tilesTransforms = pathGameObject.GetComponentsInChildren<Transform>();
            var tilesRenderers = pathGameObject.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < tilesRenderers.Length; j++)
            {
                tilesRenderers[j].material.color = colors[(int)paths[i].color];
                if (paths[i].isBuilt && IsHost)
                {
                    var pom = Instantiate(gameManager.shipGameObjectList[paths[i].builtById], tilesTransforms[j + 1].position, tilesTransforms[j + 1].rotation);
                    Debug.Log(pom);
                    pom.GetComponent<Move>().speed = 0;
                    pom.GetComponent<NetworkObject>().Spawn(true);
                }
            }
        }
        
    }
    [ClientRpc]
    public void SetMapDataClientRpc()
    {
        //Debug.Log("SetMapDataClientRpc");
        canvasForPlanetsNames = GameObject.Find("CanvasForPlanetsNames").GetComponent<Canvas>();
        //Debug.Log(canvasForPlanetsNames);
        //mapData = Communication.availableMapsData[mapNumber];
        //Debug.Log(mapData);
        //paths = mapData.paths;
        planets = mapData.planets;
        missions = mapData.missions;
        CreateMap();
       // Debug.Log(mapData);
    }

    [ClientRpc]
    public void SetPathsClientRpc(int id,string planetFromName,string planetToName, Color color, int length, bool isBuilt, int builtById,int mapNumber)
    {
        paths ??= new();
        mapData = mapData != null ? mapData : Communication.availableMapsData[mapNumber];
        Planet planetFrom = mapData.planets.Single(planet => planet.name == planetFromName);
        Planet planetTo = mapData.planets.Single(planet => planet.name == planetToName);
        Path path = Path.CreateInstance(id,planetFrom,planetTo, color,length,isBuilt,builtById);
        //Debug.Log($"Path {path.planetFrom.name}-{path.planetTo.name} jest zbudowana? {path.isBuilt}");
        paths.Add(path);
    }
}