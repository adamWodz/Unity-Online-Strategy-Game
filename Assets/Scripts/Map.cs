using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class Map : NetworkBehaviour, IDataPersistence
{
    public static MapData mapData;
    public MapData m_mapData;

    public static bool[] loadBuildPath = new bool[100];

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
    public GameObject circlePrefab;
    public GameObject squarePrefab;

    private UnityEngine.Color[] colors = new UnityEngine.Color[] {
        UnityEngine.Color.red,
        UnityEngine.Color.green,
        UnityEngine.Color.blue,
        UnityEngine.Color.yellow,
        UnityEngine.Color.magenta,
        UnityEngine.Color.grey,
    };

    private Canvas canvasForPlanetsNames;

    private GameManager gameManager;

    private float distanceBetweenPaths = 0.1f;
    void Start()
    {
        mapData = m_mapData;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        missions = mapData.missions;
        if (!Communication.loadOnStart)
        {
            canvasForPlanetsNames = GameObject.Find("CanvasForPlanetsNames").GetComponent<Canvas>();
            paths = mapData.paths;
            planets = mapData.planets;
            
            CreateMap();
        }
        else
            paths = new();

        Server.allMissions = new List<Mission>();
        foreach (Mission mission in mapData.missions)
            Server.allMissions.Add(mission);
        
    }

    public void LoadData(GameData data)
    {
        if (IsHost)
        {
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
            if (data.paths.Count == 0)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    PathData pathData = new()
                    {
                        id = paths[i].Id,
                        planetFromName = paths[i].planetFrom.name,
                        planetToName = paths[i].planetTo.name,
                        color = paths[i].color,
                        length = paths[i].length,
                        isBuilt = paths[i].isBuilt,
                        builtById = paths[i].builtById == -1 ? mapData.paths[i].builtById : paths[i].builtById,
                    };
                    data.paths.Add(pathData);
                }
            }
            else
            {
                var donePaths = paths.Where(p => p.isBuilt).ToList();
                var donePaths2 = mapData.paths.Where(p => p.isBuilt).ToList();
                foreach (var p in donePaths2)
                {
                    var pom = data.paths.FindIndex(s => s.id == p.Id);
                    if (pom != -1)
                        data.paths[pom] = new() 
                        {
                            id = p.Id,
                            planetFromName = p.planetFrom.name,
                            planetToName= p.planetTo.name,
                            color = p.color,
                            length = p.length,
                            isBuilt = p.isBuilt,
                            builtById = p.builtById
                        };
                }
            }
            data.mapNumber = Communication.mapDataNumber;
        }
    }

    void CreateMap()
    {
        bool[] pathWasSpawned = new bool[paths.Count];
        for (int i = 0; i < planets.Count; i++)
        {
            var planet = Instantiate(planets[i].planetPrefab, new Vector3(planets[i].positionX, planets[i].positionY, mapZparam), planets[i].planetPrefab.transform.rotation);
            planet.name = planets[i].name;

            var higlighten = Instantiate(circlePrefab, planet.transform);
            higlighten.transform.localScale = new Vector3(7, 7, 1);
            higlighten.transform.localPosition = new Vector3(0, 0, 0.74f);
            higlighten.SetActive(false);

            var planetNameText = Instantiate(this.planetNameText, new Vector3(planets[i].positionX, planets[i].positionY + 0.5f, mapZparam), Quaternion.identity);
            planetNameText.GetComponent<TMP_Text>().text = planets[i].name;
            planetNameText.name = planets[i].name + "Text";
            planetNameText.transform.SetParent(canvasForPlanetsNames.transform);
            if(gameManager == null)
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            gameManager.spawnedObjects.Add(planet);
            gameManager.spawnedObjects.Add(planetNameText);
        }

        for (int i = 0; i < paths.Count; i++)
        {
            if (!pathWasSpawned[i])           
            {
                pathWasSpawned[i] = true;
                var path = paths[i];
                float angle = Mathf.Atan2(
                              paths[i].planetTo.positionX - paths[i].planetFrom.positionX,
                              paths[i].planetTo.positionY - paths[i].planetFrom.positionY
                              ) * Mathf.Rad2Deg;

                Vector2 position = Vector2.Lerp(new Vector2(paths[i].planetTo.positionX, paths[i].planetTo.positionY),
                    new Vector2(paths[i].planetFrom.positionX, paths[i].planetFrom.positionY), 0.5f);

                int k = paths.FindIndex(path3 => path3.IsEqualByName(path) && path3.Id != path.Id);
                if (k != -1 && !pathWasSpawned[k])
                {
                    pathWasSpawned[k] = true;
                    var path2 = paths[k];
                    Vector2 planetPosition = new(path2.planetTo.positionX, path2.planetTo.positionY);
                    Vector2 perpendicularDirection = Vector2.Perpendicular(planetPosition - position).normalized;
                    Vector2 target = position + perpendicularDirection * distanceBetweenPaths;
                    
                    SpawnPath(path2, target, angle);

                    position -= perpendicularDirection * distanceBetweenPaths;
  
                }
                SpawnPath(path, position, angle);
            }
        }
    }
    void SpawnPath(Path path, Vector2 position, float angle)
    {
        var pathGameObject = Instantiate(pathsPrefabs[path.length - 1], position, Quaternion.Euler(new Vector3(0, 0, -angle)));
        
        var highlight = Instantiate(squarePrefab, pathGameObject.transform);
        highlight.transform.localScale = new(0.2f,path.length*0.5f,1);
        highlight.transform.localPosition = new(0, 0, -0.069f);
        highlight.SetActive(false);

        var buildPath = pathGameObject.GetComponent<BuildPath>();
        buildPath.path = path;
        Server.buildPaths.Add(buildPath);

        var tilesTransforms = pathGameObject.GetComponentsInChildren<Transform>();
        var tilesRenderers = pathGameObject.GetComponentsInChildren<Renderer>();
        for (int j = 0; j < tilesRenderers.Length; j++)
        {
            tilesRenderers[j].material.color = colors[(int)path.color];
            if (path.isBuilt && IsHost && path.builtById != -1)
            {
                    var pom = Instantiate(gameManager.shipGameObjectList[Server.allPlayersInfo.Where(p => p.Id == path.builtById).FirstOrDefault().ColorNum], tilesTransforms[j + 1].position, tilesTransforms[j + 1].rotation);
                    pom.GetComponent<Move>().speed = 0;
                    pom.GetComponent<NetworkObject>().Spawn(true);
                    gameManager.spawnedSpaceships ??= new();
                    gameManager.spawnedSpaceships.Add(pom.GetComponent<NetworkObject>());
            }
        }

        gameManager.spawnedObjects.Add(pathGameObject);
    }

    [ClientRpc]
    public void SetMapDataClientRpc()
    {
        canvasForPlanetsNames = GameObject.Find("CanvasForPlanetsNames").GetComponent<Canvas>();
        planets = mapData.planets;
        missions = mapData.missions;
        CreateMap();
    }

    [ClientRpc]
    public void SetPathsClientRpc(int id,string planetFromName,string planetToName, Color color, int length, bool isBuilt, int builtById,int mapNumber)
    {
        paths ??= new();
        mapData = mapData != null ? mapData : Communication.availableMapsData[mapNumber];
        Planet planetFrom = mapData.planets.Single(planet => planet.name == planetFromName);
        Planet planetTo = mapData.planets.Single(planet => planet.name == planetToName);
        Path path = Path.CreateInstance(id,planetFrom,planetTo, color,length,isBuilt,builtById);
        loadBuildPath[id] = isBuilt;
        paths.Add(path);
    }
}