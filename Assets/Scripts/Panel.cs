using System;
using UnityEngine;
using UnityEngine.UI;

public enum PanelState
{
    Minimized,
    Maximized,
    Rolling
};

public class Panel : MonoBehaviour
{
    private float speed = 500;
    
    protected Button button;
    protected RectTransform panel;
    protected float width;
    protected float maxWidth;
    protected float minWidth;
    public PanelState panelState;

    protected void AssignValues(float minW, float maxW, PanelState state, bool getParentRectTransform)
    {
        //ustawiam maksymalna i minimalna dlugosc panelu
        maxWidth = maxW;
        minWidth = minW;

        //ustawiam pocz¹tkowy syan panelu
        panelState = state;

        // pobieram odpowiedni przycisk i nadaje mu funkcje
        button = GameObject.Find("DrawMissionsCardsButton").GetComponent<Button>();
        button.onClick.AddListener(() => ChangeState());

        // pobieram transforme panelu i zapisuje jej dlugosc
        panel = getParentRectTransform ? transform.parent.GetComponent<RectTransform>() : GetComponent<RectTransform>();
        width = panel.sizeDelta.x;
    }

    // funkcja zmieniajaca stan panelu w grze
    protected void ChangeState()
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
}
