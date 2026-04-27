using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMinMaxSize : MonoBehaviour
{
    public Vector2 minSize;
    public Vector2 maxFactor;
    RectTransform rt, canvasRT;

    Vector2 lastAnchoredPos, lastSize;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        canvasRT = rt.parent.GetComponent<RectTransform>();
    }
    
    void LateUpdate()
    {
        if(rt.GetWidth() < minSize.x) {
            rt.SetWidth(minSize.x);
        }
        if(rt.GetHeight() < minSize.y) {
            rt.SetHeight(minSize.y);
        }

        if(rt.GetWidth() > Screen.width * maxFactor.x) {
            rt.SetWidth(Screen.width * maxFactor.x);
        }
        if(rt.GetHeight() > Screen.height * maxFactor.y) {
            rt.SetHeight(Screen.height * maxFactor.y);
        }

        Vector2[] corners = new Vector2[4];
        corners[0] = rt.anchoredPosition;
        corners[1] = new Vector2(rt.anchoredPosition.x + rt.GetWidth(), rt.anchoredPosition.y);
        corners[2] = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + rt.GetHeight());
        corners[3] = new Vector2(rt.anchoredPosition.x + rt.GetWidth(), rt.anchoredPosition.y + rt.GetHeight());
        
        for(int i=0;i<corners.Length; i++) {
            Vector2 local = canvasRT.InverseTransformPoint(corners[i]);
            if (!canvasRT.rect.Contains(local)) {
                rt.anchoredPosition = lastAnchoredPos;
                rt.SetSize(lastSize);
                break;
            }
        }

        lastAnchoredPos = rt.anchoredPosition;
        lastSize = new Vector2(rt.GetWidth(), rt.GetHeight());
    }
}
