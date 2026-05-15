using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomCursor : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler {

    public Texture2D cursorEnter, cursorExit;
    public Vector2 cursorEnterOffset, cursorExitOffset;

    public void OnPointerEnter(PointerEventData eventData) {
        if(cursorEnter != null)
            Cursor.SetCursor(cursorEnter, cursorEnterOffset, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if(cursorExit != null)
            Cursor.SetCursor(cursorExit, cursorExitOffset, CursorMode.Auto);
    }
}
