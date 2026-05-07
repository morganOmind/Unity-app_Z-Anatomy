using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelStayOnScreen : MonoBehaviour
{
    public Vector2 stayOnScreenPos;
    Vector2 initPos;
    RectTransform rt, parentRT;
    public RectTransform canvasRT;

    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
        initPos = rt.anchoredPosition;
        parentRT = rt.parent.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        rt.anchoredPosition = initPos;

        Vector2[] corners = new Vector2[4];
        corners[0] = rt.localPosition;
        corners[1] = new Vector2(rt.localPosition.x - rt.GetWidth(), rt.localPosition.y);
        corners[2] = new Vector2(rt.localPosition.x, rt.localPosition.y + rt.GetHeight());
        corners[3] = new Vector2(rt.localPosition.x - rt.GetWidth(), rt.localPosition.y + rt.GetHeight());

        bool isOffScreen = false;
        for (int i = 0; i < corners.Length; i++) {
            Vector2 local = canvasRT.InverseTransformPoint(parentRT.TransformPoint(corners[i]));
            if (!canvasRT.rect.Contains(local)) {
                isOffScreen = true;
                break;
            }
        }
        if (isOffScreen) {
            rt.anchoredPosition = stayOnScreenPos;
        }
    }
}
