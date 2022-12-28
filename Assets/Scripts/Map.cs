using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private float mapZparam = -1;
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
            //Id = 0,
            positionX = -5,
            positionY = 3
        };
        Planet wenus = new()
        {
            name = "Wenus",
            //Id = 1,
            positionX = 0.5f,
            positionY = 3
        };
        Planet ziemia = new()
        {
            name = "Ziemia",
            //Id = 2,
            positionX = 0.48f,
            positionY = 0.06f
        };
        Planet ksiezyc = new()
        {
            name = "Ksi??yc",
            //Id = 3,
            positionX = 1.009f,
            positionY = -1.07f
        };
        Planet mars = new()
        {
            name = "Mars",
            //Id = 4,
            positionX = 2.84f,
            positionY = 1.29f
        };
        Planet jowisz = new()
        {
            name = "Jowisz",
            //Id = 5,
            positionX = -5,
            positionY = -2.4f
        };
        Planet saturn = new()
        {
            name = "Saturn",
            //Id = 6,
            positionX = 5.75f,
            positionY = -2.16f
        };
        Planet uran = new()
        {
            name = "Uran",
            //Id = 7,
            positionX = -2.85f,
            positionY = 0.96f
        };
        Planet neptun = new()
        {
            name = "Neptun",
            //Id = 8,
            positionX = 5.75f,
            positionY = 2.56f
        };
        Planet pluton = new()
        {
            name = "Pluton",
            //Id = 9,
            positionX = 6.67f,
            positionY = 0.07f
        };
        Planet planeta = new()
        {
            name = "Planeta",
            //Id = 10,
            positionX = -1.8f,
            positionY = -2.04f
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
        for (int i = 0; i < planets.Count; i++)
        {
            Instantiate(planetsPrefabs[i], new Vector3(planets[i].positionX, planets[i].positionY, mapZparam), planetsPrefabs[i].transform.rotation);
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