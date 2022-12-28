using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum PanelState
{
    Minimized,
    Maximized,
    Rolling
};
// klasa por�wnuj�ca �cie�ki na podstawie nazw planet znajduj�cych si� na tej �cie�ce
// potrzebna przy funcji Except dost�pnej dla List
public class PathComparer: IEqualityComparer<Path>
{
    public int GetHashCode(Path path)
    {
        if(path == null)
            return 0;
        return path.planetFrom.GetHashCode() + path.planetTo.GetHashCode();
    }

    public bool Equals(Path x, Path y)
    {
        if(ReferenceEquals(x, y)) 
            return true;
        if(x is null || y is null) 
            return false;
        return x.planetTo.name == y.planetTo.name && x.planetFrom.name == y.planetFrom.name;
    }
}

public class Panel : MonoBehaviour
{
    private float speed = 500;
    
    public GameObject popUpPanel;

    protected Button button;
    protected RectTransform panel;
    protected float width;
    protected float maxWidth;
    protected float minWidth;
    public PanelState panelState;
    
    protected void AssignValues(float minW, float maxW, PanelState state, bool getParentRectTransform)
    {
        popUpPanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ChangeState());
        popUpPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => CancelButtonWasClicked());
        
        //ustawiam maksymalna i minimalna dlugosc panelu
        maxWidth = maxW;
        minWidth = minW;

        //ustawiam pocz�tkowy syan panelu
        panelState = state;

        // pobieram odpowiedni przycisk i nadaje mu funkcje
        button = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
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
        button.enabled = false;
    }

    // funkcja zmieniajaca dlugosc panelu w grze
    protected void ChangeWidth()
    {
        if (panelState == PanelState.Rolling)
        {
            panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
            width += speed * Time.deltaTime;

            if (width >= maxWidth)
            {
                panelState = PanelState.Maximized;
            }
            if (width <= minWidth)
            {
                panelState = PanelState.Minimized;
            }
        }
    }

    private void CancelButtonWasClicked()
    {
        popUpPanel.SetActive(false);
    }

    
}
