using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathsPanel : MonoBehaviour
{
    
    public List<string> paths;
    // Start is called before the first frame update
    void Start()
    {
        GameObject playerTextTempalte = transform.GetChild(0).gameObject;
        GameObject path;

        int n = paths.Count;

        for (int i = 0; i < n; i++)
        {
            path = Instantiate(playerTextTempalte, transform);
            path.transform.GetChild(0).GetComponent<TMP_Text>().text = paths[i];
        }

        Destroy(playerTextTempalte);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
