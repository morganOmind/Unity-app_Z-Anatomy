using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public enum BoxAnchorDirection {
    Left, Right, Up, Down,
    LeftUp, LeftDown, RightUp, RightDown
};

public class BoxAnchor : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerExitHandler, IPointerEnterHandler {
    public BoxAnchorDirection direction;
    public RectTransform boxRT;
    public Texture2D dragCursorTexture;
    public Vector2 cursorOffset;
    bool dragging;

    public void OnPointerEnter(PointerEventData eventData) {
        Cursor.SetCursor(dragCursorTexture, cursorOffset, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData) {
        StartCoroutine(WaitForEndDrag());
    }

    IEnumerator WaitForEndDrag() {
        yield return new WaitUntil(() => !dragging);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        dragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        switch (direction) {
            case BoxAnchorDirection.Left:
                boxRT.SetLeft(boxRT.GetLeft() + eventData.delta.x);
                break;
            case BoxAnchorDirection.Right:
                boxRT.SetRight(boxRT.GetRight() - eventData.delta.x);
                break;
            case BoxAnchorDirection.Up:
                boxRT.SetTop(boxRT.GetTop() - eventData.delta.y);
                break;
            case BoxAnchorDirection.Down:
                boxRT.SetBottom(boxRT.GetBottom() + eventData.delta.y);
                break;
            case BoxAnchorDirection.LeftUp:
                boxRT.SetLeft(boxRT.GetLeft() + eventData.delta.x);
                boxRT.SetTop(boxRT.GetTop() - eventData.delta.y);
                break;
            case BoxAnchorDirection.LeftDown:
                boxRT.SetLeft(boxRT.GetLeft() + eventData.delta.x);
                boxRT.SetBottom(boxRT.GetBottom() + eventData.delta.y);
                break;
            case BoxAnchorDirection.RightUp:
                boxRT.SetRight(boxRT.GetRight() - eventData.delta.x);
                boxRT.SetTop(boxRT.GetTop() - eventData.delta.y);
                break;
            case BoxAnchorDirection.RightDown:
                boxRT.SetRight(boxRT.GetRight() - eventData.delta.x);
                boxRT.SetBottom(boxRT.GetBottom() + eventData.delta.y);
                break;
            default:break;
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        dragging = false;
    }
}
