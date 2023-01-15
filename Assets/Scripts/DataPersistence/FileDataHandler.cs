using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "word";

    public FileDataHandler(string dataDirPath,string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        string fullPath = System.IO.Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;

        if(File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new(fullPath,FileMode.Open))
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // optional decryption of data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // deserialize the data from Json
                loadedData = JsonConvert.DeserializeObject<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = System.IO.Path.Combine(dataDirPath,dataFileName);
        try
        {
            // create the directory the file will be written to if it does not already exist
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));

            // serialize the game data object into Json 

            // Formatting.Indented, ¿eby móc rozczytaæ zapisany plik
            string dataToStore = JsonConvert.SerializeObject(data,Formatting.Indented); // lepsza funkcja: potrafi zserializowaæ s³owniki, listy list itp.

            // optional encryption of data
            if(useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized data to the file
            using(FileStream stream = new(fullPath,FileMode.Create))
            {
                using(StreamWriter writer = new(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    // XOR encryption of data
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for(int i=0;i<data.Length;i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
}
