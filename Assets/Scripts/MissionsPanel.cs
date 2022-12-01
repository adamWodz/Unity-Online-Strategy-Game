using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionsPanel : MonoBehaviour
{
    RectTransform panel;
    float width;
    float speed = 100;
    float maxWidth = 191.84f;
    float minWidth = 0;
    private PanelState panelState = PanelState.Minimized;
    private Button button;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        //pobieram game managera
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //pobieram odpowiedni przycisk i nadajê mu funkcjê
        button = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        button.onClick.AddListener(() => { gameManager.ChangeState(ref panelState, ref speed); });
        
        panel = GetComponent<RectTransform>();
        width = panel.sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        gameManager.ChangeWidth(ref panelState,ref width,ref panel,maxWidth,minWidth,ref speed);
    }
}
