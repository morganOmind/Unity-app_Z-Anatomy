using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotMgr : MonoBehaviour
{
    public RectTransform boxCanvasRT, boxRootRT;

    bool first = true;
    public float firstBoxSizeFactor = 0.33f;
    
    public void OnScreenshot() {
        RaycastObject.instance.enabled = false;

        if (first) {
            first = false;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            Vector2 box = screen * firstBoxSizeFactor;
            Vector2 pos = (screen - box) / 2f;
            boxRootRT.SetLeft(pos.x);
            boxRootRT.SetRight(pos.x);
            boxRootRT.SetBottom(pos.y);
            boxRootRT.SetTop(box.y);
        }

        boxCanvasRT.gameObject.SetActive(true);
    }

    public void OnValidScreenshotDown() {
#if UNITY_WEBGL && !UNITY_EDITOR
        MakeScreenshot();
#endif
    }

    public void OnValidScreenshotClick() {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        MakeScreenshot();
#endif
    }

    void MakeScreenshot() {
        RaycastObject.instance.enabled = true;
        boxCanvasRT.gameObject.SetActive(false);
        GetComponent<CameraScreenshot>().CaptureScreenshot(boxRootRT);
    }

    public void OnCancelScreenshot() {
        RaycastObject.instance.enabled = true;
        boxCanvasRT.gameObject.SetActive(false);
    }
}
