using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionsPanel : Panel
{
    // Start is called before the first frame update
    void Start()
    {
        AssignValues(0, 191.84f, PanelState.Minimized, false);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth();
    }
}
