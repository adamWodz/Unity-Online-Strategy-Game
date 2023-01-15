using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : NetworkBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    public static GameData gameData;

    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene");
        }
        Instance = this;
    }

    private void Start()
    {
        dataHandler = new(Application.persistentDataPath, fileName);
        
        // znajdujemy listê skryptów, które posiadaj¹ dane do zapisu/wczytania
        dataPersistenceObjects = FindAllDataPersistenceObjects();

        if (Communication.loadOnStart)
            LoadGame();
        else
            NewGame();
        //NewGame();
    }

    public void NewGame()
    {
        gameData = new();
    }

    public void LoadGame()
    {
        // Load any saved data from file using the data handler
        gameData = dataHandler.Load();

        if(gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults");
            NewGame();
        }
        
        foreach(IDataPersistence dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject.SaveData(ref gameData);
        }
        
        dataHandler.Save(gameData);
    }
    /*
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    */
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<NetworkBehaviour>().OfType<IDataPersistence>();

        return new(dataPersistenceObjects);
    }
}
