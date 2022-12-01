using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public enum PanelState
{
    Minimized,
    Maximized,
    Rolling
};
public class GameManager : MonoBehaviour
{
    public GameObject shipGameObject;

    private List<GameObject> shipList = new();
    private List<Transform> shipTransformList = new();
    private Vector3 spaceshipsBase = new(-8, -4, -1);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeState(ref PanelState panelState, ref float speed)
    {
        switch (panelState)
        {
            case PanelState.Minimized:
                speed = 100;
                panelState = PanelState.Rolling;
                break;
            case PanelState.Maximized:
                speed = -100;
                panelState = PanelState.Rolling;
                break;
        }
    }
    // funkcja zmieniaj¹ca d³ugoœæ paneli UI w grze
    public void ChangeWidth(ref PanelState panelState, ref float width, ref RectTransform panel, float maxWidth, float minWidth, ref float speed)
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
    public void SpawnShips(Transform t)
    {
        var spawnedShipGameObject = Instantiate(shipGameObject, spaceshipsBase, t.rotation);
        var spawnedShip = spawnedShipGameObject.GetComponent<Move>();
        spawnedShip.shipGoal = t;
        spawnedShip.move = true;
    }
}
