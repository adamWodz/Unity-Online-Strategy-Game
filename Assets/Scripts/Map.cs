using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public List<Path> paths;
    public List<Planet> planets;
    public GameObject[] pathsPrefabs;
    public GameObject[] planetsPrefabs;

    // kolory sciezek
    private Dictionary<Assets.GameplayControl.Color, UnityEngine.Color> colorOnMap = new Dictionary<Assets.GameplayControl.Color, UnityEngine.Color>
    {
        { Assets.GameplayControl.Color.red, UnityEngine.Color.red },
        { Assets.GameplayControl.Color.green, UnityEngine.Color.green },
        { Assets.GameplayControl.Color.blue, UnityEngine.Color.blue },
        { Assets.GameplayControl.Color.black, UnityEngine.Color.black },
        { Assets.GameplayControl.Color.white, UnityEngine.Color.white },
        { Assets.GameplayControl.Color.yellow, UnityEngine.Color.yellow },
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
                //Id = 0,
                planetFrom = merkury,
                planetTo = wenus,
                color = Assets.GameplayControl.Color.red,
                length = 8,
            },
            new Path()
            {
                //Id = 1,
                planetFrom = merkury,
                planetTo = uran,
                color = Assets.GameplayControl.Color.blue,
                length = 4
            },
            new Path()
            {
                //Id = 2,
                planetFrom = uran,
                planetTo = ziemia,
                color = Assets.GameplayControl.Color.black,
                length = 5
            },
            new Path()
            {
                //Id = 3,
                planetFrom = ziemia,
                planetTo = mars,
                color = Assets.GameplayControl.Color.white,
                length = 3
            },
            new Path()
            {
                //Id = 4,
                planetFrom = mars,
                planetTo = pluton,
                color = Assets.GameplayControl.Color.red,
                length = 6
            },
            new Path()
            {
                //Id = 5,
                planetFrom = pluton,
                planetTo = saturn,
                color = Assets.GameplayControl.Color.green,
                length = 3
            },
            new Path()
            {
                //Id = 6,
                planetFrom = mars,
                planetTo = wenus,
                color = Assets.GameplayControl.Color.yellow,
                length = 4
            },
            new Path()
            {
                //Id = 7,
                planetFrom = ziemia,
                planetTo = ksiezyc,
                color = Assets.GameplayControl.Color.green,
                length = 1
            },
            new Path()
            {
                //Id = 8,
                planetFrom = uran,
                planetTo = planeta,
                color = Assets.GameplayControl.Color.pink,
                length = 4
            },
            new Path()
            {
                //Id = 9,
                planetFrom = planeta,
                planetTo = jowisz,
                color = Assets.GameplayControl.Color.special,
                length = 5
            },
            new Path()
            {
                //Id = 10,
                planetFrom = mars,
                planetTo = neptun,
                color = Assets.GameplayControl.Color.special,
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
            Instantiate(planetsPrefabs[i], planets[i].position, planetsPrefabs[i].transform.rotation);
        }

        // Tworzenie œcie¿ek
        for (int i = 0; i < paths.Count; i++)
        {
            // k¹t nachylenia miêdzy dwoma planetami
            float angle = Mathf.Atan2(
                          paths[i].planetTo.position.x - paths[i].planetFrom.position.x,
                          paths[i].planetTo.position.y - paths[i].planetFrom.position.y
                          ) * Mathf.Rad2Deg;

            // œrodkowy punkt pomiêdzy planetami
            Vector2 position = Vector2.Lerp(paths[i].planetTo.position, paths[i].planetFrom.position, 0.5f);

            var pathGameObject = Instantiate(pathsPrefabs[paths[i].length - 1], position, Quaternion.Euler(new Vector3(0, 0, -angle)));
            // przypisanie do build path
            var buildPath = pathGameObject.GetComponent<BuildPath>();
            buildPath.path = paths[i];

            var tilesRenderers = pathGameObject.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < tilesRenderers.Length; j++)
            {
                tilesRenderers[j].material.color = colorOnMap[(Assets.GameplayControl.Color)(i % colorOnMap.Count)];
            }
        }
    }
}
