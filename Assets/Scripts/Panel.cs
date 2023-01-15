using Assets.GameplayControl;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum PanelState
{
    Minimized,
    Maximized,
    Rolling
};
// klasa porównuj¹ca œcie¿ki na podstawie nazw planet znajduj¹cych siê na tej œcie¿ce
// potrzebna przy funcji Except dostêpnej dla List
public class MissionComparer: IEqualityComparer<Mission>
{
    public int GetHashCode(Mission mission)
    {
        if(mission == null)
            return 0;
        return mission.start.GetHashCode() + mission.end.GetHashCode();
    }

    public bool Equals(Mission x, Mission y)
    {
        if(ReferenceEquals(x, y)) 
            return true;
        if(x == null || y == null) 
            return false;
        return x.end.name == y.end.name && x.start.name == y.start.name;
    }
}

public class Panel : NetworkBehaviour, IDataPersistence
{
    private float speed = 500;
    
    public GameObject popUpPanel;

    protected Button drawMissionsCardsButton;
    protected RectTransform panel;
    protected float width;
    protected float maxWidth;
    protected float minWidth;
    public PanelState panelState;
    
    protected void AssignValues(float minW, float maxW, PanelState state, bool getParentRectTransform)
    {
        popUpPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                ChangeState();
                PlayerGameData.isDrawingMission = true;
            });
        popUpPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => CancelButtonWasClicked());
        
        //ustawiam maksymalna i minimalna dlugosc panelu
        maxWidth = maxW;
        minWidth = minW;

        //ustawiam pocz¹tkowy syan panelu
        panelState = state;

        // pobieram odpowiedni przycisk i nadaje mu funkcje
        drawMissionsCardsButton = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        //button.onClick.AddListener(() => ChangeState());

        // pobieram transforme panelu i zapisuje jej dlugosc
        panel = getParentRectTransform ? transform.parent.GetComponent<RectTransform>() : GetComponent<RectTransform>();
        width = panel.sizeDelta.x;
    }

    // funkcja zmieniajaca stan panelu w grze
    public void ChangeState()
    {   
        switch (panelState)
            {
                case PanelState.Minimized:
                    speed = Math.Abs(speed);
                    panelState = PanelState.Rolling;
                    break;
                case PanelState.Maximized:
                    speed = -Math.Abs(speed);
                    panelState = PanelState.Rolling;
                    break;
            }

        popUpPanel.SetActive(false);
        drawMissionsCardsButton.enabled = false;
    }

    // funkcja zmieniajaca dlugosc panelu w grze
    protected void ChangeWidth()
    {
        if (panelState == PanelState.Rolling)
        {
            panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
            width += (int)(speed * Time.deltaTime);

            if (width >= maxWidth)
            {
                panelState = PanelState.Maximized;
                maxWidth = width;
            }
            if (width <= minWidth)
            {
                panelState = PanelState.Minimized;
                minWidth = width;
            }
        }
    }

    private void CancelButtonWasClicked()
    {
        popUpPanel.SetActive(false);
    }

    public virtual void LoadData(GameData gameData)
    {

    }

    public virtual void SaveData(ref GameData gameData)
    {

    }
    
}
