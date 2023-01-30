using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : NetworkBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public static GameData gameData;

    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
        }
        Instance = this;
    }

    private void Start()
    {
        dataHandler = new(Application.persistentDataPath, fileName,useEncryption);
        
        dataPersistenceObjects = FindAllDataPersistenceObjects();

        if (Communication.loadOnStart)
            LoadGame();
        else
            NewGame();
    }

    public void NewGame()
    {
        gameData = new();
    }

    public void LoadGame()
    {
        gameData = dataHandler.Load();

        if(gameData == null)
        {
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
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<NetworkBehaviour>().OfType<IDataPersistence>();

        return new(dataPersistenceObjects);
    }
}
