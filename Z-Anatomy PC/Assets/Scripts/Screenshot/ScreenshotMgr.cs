using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotMgr : MonoBehaviour
{
    public RectTransform boxCanvasRT, boxRootRT;

    bool first = true;
    public float firstBoxSizeFactor = 0.33f;
    
    List<MonoBehaviour> disableComponents;

    public Texture2D cursor;
    public Vector2 cursorOffset;

    private void Start() {
        disableComponents = new List<MonoBehaviour>();
        disableComponents.Add(RaycastObject.instance);
        disableComponents.Add(BrushSelection.instance);
        disableComponents.Add(BoxSelection.instance);
        disableComponents.Add(LassoSelection.instance);

        disableComponents.AddRange(FindObjectsOfType<ExpandCollapseUI>());
        disableComponents.AddRange(FindObjectsOfType<GraphicRaycaster>());

        disableComponents.Remove(boxRootRT.GetComponent<GraphicRaycaster>());
    }

    public void OnScreenshot() {
        foreach(MonoBehaviour m in disableComponents) {
            m.enabled = false;
        }

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

        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
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
        ResetStuff();
        GetComponent<CameraScreenshot>().CaptureScreenshot(boxRootRT);
    }

    public void OnCancelScreenshot() {
        ResetStuff();
    }

    void ResetStuff() {
        foreach (MonoBehaviour m in disableComponents) {
            m.enabled = true;
        }

        boxCanvasRT.gameObject.SetActive(false);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
