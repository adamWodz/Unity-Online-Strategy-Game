using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
    public CanvasGroup panel;
    
    bool showingUp = false;
    bool fadingOut = false;

    public void ShowUp()
    {
        showingUp = true;
    }

    public void FadeOut()
    {
        fadingOut = true;
    }

    void Update()
    {
        if(showingUp)
        {
            Debug.Log("Shwoing up");
            if (panel.alpha < 1)
            {
                panel.alpha += Time.deltaTime;
                if(panel.alpha >= 1)
                    showingUp = false;
            }
        }

        else if(fadingOut)
        {
            Debug.Log("Fading out");
            if (panel.alpha >= 0)
            {
                panel.alpha -= Time.deltaTime;
                if (panel.alpha == 0)
                    fadingOut = false;
            }
        }
    }
}
