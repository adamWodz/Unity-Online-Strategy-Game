using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<Path> paths;
    public List<Path> Paths
    { 
        get { return paths; } 
    }
    private List<Planet> planets;
    public GameObject[] pathsPrefabs;
    public GameObject[] planetsPrefabs;
    
    // punkty krancowe mapy
    Vector2 leftTopPoint = new(-5.8f, 3.6f);
    Vector2 rightTopPoint = new(7, 3.6f);
    Vector2 leftBottomPoint = new(-5.8f, -3.1f);
    Vector2 rightPoint = new(7, -3.1f);

    // kolory sciezek
    private UnityEngine.Color[] colors = new UnityEngine.Color[] { 
        UnityEngine.Color.red,
        UnityEngine.Color.green,
        UnityEngine.Color.blue,
        UnityEngine.Color.black,
        UnityEngine.Color.white,
        UnityEngine.Color.yellow,
        UnityEngine.Color.magenta,
        UnityEngine.Color.grey,
    };

    // Start is called before the first frame update
    void Start()
    {
        Planet merkury = new()
        {
            name = "Merkury",
            Id = 0,
            position = new Vector3(-5, 3, -1)
        };
        Planet wenus = new()
        {
            name = "Wenus",
            Id = 1,
            position = new Vector3(0.5f, 3, -1)
        };
        Planet ziemia = new()
        {
            name = "Ziemia",
            Id = 2,
            position = new Vector3(0.48f, 0.06f, -1)
        };
        Planet ksiezyc = new()
        {
            name = "Ksiê¿yc",
            Id = 3,
            position = new Vector3(1.009f, -1.07f, -1)
        };
        Planet mars = new()
        {
            name = "Mars",
            Id = 4,
            position = new Vector3(2.84f, 1.29f, -1)
        };
        Planet jowisz = new()
        {
            name = "Jowisz",
            Id = 5,
            position = new Vector3(-5, -2.4f, -1)
        };
        Planet saturn = new()
        {
            name = "Saturn",
            Id = 6,
            position = new Vector3(5.75f, -2.16f, -1)
        };
        Planet uran = new()
        {
            name = "Uran",
            Id = 7,
            position = new Vector3(-2.85f, 0.96f, -1)
        };
        Planet neptun = new()
        {
            name = "Neptun",
            Id = 8,
            position = new Vector3(5.75f, 2.56f, -1)
        };
        Planet pluton = new()
        {
            name = "Pluton",
            Id = 9,
            position = new Vector3(6.67f, 0.07f, -1)
        };
        Planet planeta = new()
        {
            name = "Planeta",
            Id = 10,
            position = new Vector3(-1.8f, -2.04f, -1)
        };
        
        paths = new()
        {
            new Path()
            {
                Id = 0,
                planetFrom = merkury,
                planetTo = wenus,
                color = Color.red,
                length = 8,
            },
            new Path()
            {
                Id = 1,
                planetFrom = merkury,
                planetTo = uran,
                color = Color.blue,
                length = 4
            },
            new Path()
            {
                Id = 2,
                planetFrom = uran,
                planetTo = ziemia,
                color = Color.black,
                length = 5
            },
            new Path()
            {
                Id = 3,
                planetFrom = ziemia,
                planetTo = mars,
                color = Color.white,
                length = 3
            },
            new Path()
            {
                Id = 4,
                planetFrom = mars,
                planetTo = pluton,
                color = Color.red,
                length = 6
            },
            new Path()
            {
                Id = 5,
                planetFrom = pluton,
                planetTo = saturn,
                color = Color.green,
                length = 3
            },
            new Path()
            {
                Id = 6,
                planetFrom = mars,
                planetTo = wenus,
                color = Color.yellow,
                length = 4
            },
            new Path()
            {
                Id = 7,
                planetFrom = ziemia,
                planetTo = ksiezyc,
                color = Color.green,
                length = 1
            },
            new Path()
            {
                Id = 8,
                planetFrom = uran,
                planetTo = planeta,
                color = Color.pink,
                length = 4
            },
            new Path()
            {
                Id = 9,
                planetFrom = planeta,
                planetTo = jowisz,
                color = Color.special,
                length = 5
            },
            new Path()
            {
                Id = 10,
                planetFrom = mars,
                planetTo = neptun,
                color = Color.special,
                length = 4
            },
        };

        merkury.adjacentPaths = new List<Path>()
        {
            paths[0],
            paths[1]
        };

        wenus.adjacentPaths = new List<Path>()
        {
            paths[0],
            paths[6]
        };

        ziemia.adjacentPaths = new List<Path>()
        {
            paths[2],
            paths[3],
            paths[7]
        };

        ksiezyc.adjacentPaths = new List<Path>()
        {
            paths[7]
        };

        mars.adjacentPaths = new List<Path>()
        {
            paths[3],
            paths[4],
            paths[6],
            paths[10]
        };

        jowisz.adjacentPaths = new List<Path>()
        {
            paths[9]
        };

        saturn.adjacentPaths = new List<Path>()
        {
            paths[5]
        };

        uran.adjacentPaths = new List<Path>()
        {
            paths[1],
            paths[2]
        };

        neptun.adjacentPaths = new List<Path>()
        {
            paths[10]
        };

        pluton.adjacentPaths = new List<Path>()
        {
            paths[4],
            paths[5]
        };

        planeta.adjacentPaths = new List<Path>()
        {
            paths[8],
            paths[9]
        };

        planets = new()
        {
            merkury,
            wenus,
            ziemia,
            ksiezyc,
            mars,
            jowisz,
            saturn,
            uran,
            neptun,
            pluton, 
            planeta,
        };

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
            Instantiate(planetsPrefabs[i], planets[i].position, planetsPrefabs[i].transform.rotation);
        }

        // Tworzenie œcie¿ek
        for(int i = 0;i < paths.Count; i++)
        {
            // k¹t nachylenia miêdzy dwoma planetami
            /*
            float angle = Mathf.Atan2(
                          planets[paths[i].planetsIds[1]].position.x - planets[paths[i].planetsIds[0]].position.x, 
                          planets[paths[i].planetsIds[1]].position.y - planets[paths[i].planetsIds[0]].position.y
                          ) * Mathf.Rad2Deg; 
            */
            float angle = Mathf.Atan2(
                          paths[i].planetTo.position.x - paths[i].planetFrom.position.x,
                          paths[i].planetTo.position.y - paths[i].planetFrom.position.y
                          ) * Mathf.Rad2Deg;
            //Debug.Log($"Planet{paths[i].planetsIds[0]} position: {planets[paths[i].planetsIds[0]].position}");
            //Debug.Log($"Planet{paths[i].planetsIds[1]} position: {planets[paths[i].planetsIds[1]].position}");
            //Debug.Log($"Planets {paths[i].planetsIds[0]} - {paths[i].planetsIds[1]} angle: {angle}");

            // œrodkowy punkt pomiêdzy planetami
            Vector2 position = Vector2.Lerp(paths[i].planetTo.position, paths[i].planetFrom.position, 0.5f); 
            
            var pathGameObject = Instantiate(pathsPrefabs[paths[i].length - 1], position, Quaternion.Euler(new Vector3(0,0,-angle)));
            var tilesRenderers = pathGameObject.GetComponentsInChildren<Renderer>();
            for(int j = 0; j < tilesRenderers.Length; j++)
            {
                tilesRenderers[j].material.color = colors[(int)paths[i].color];
            }
        }
    }
}
