using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PathsPanel : MonoBehaviour
{
    public List<string> paths;
    RectTransform scrollPanel;
    float maxWidth = 611.61f;
    float minWidth = 419.77f;
    float scrollWidth;
    float speed;
    private PanelState panelState = PanelState.Maximized;
    private Button button;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        //pobieram game managera
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        //pobieram odpowiedni przycisk i nadajê mu funkcjê
        button = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        button.onClick.AddListener(() => { gameManager.ChangeState(ref panelState,ref speed); });
        
        // pobieram RectTransform z PathsScroll, który jest rodzicem PathsPanel
        scrollPanel = transform.parent.GetComponent<RectTransform>();
        scrollWidth= scrollPanel.sizeDelta.x;
        
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
        gameManager.ChangeWidth(ref panelState, ref scrollWidth, ref scrollPanel, maxWidth,minWidth,ref speed); 
    }
}
