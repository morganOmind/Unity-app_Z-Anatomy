using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxMaker : MonoBehaviour
{
    public List<RectTransform> excludedRT;
    public RectTransform boxRT;

    bool isMakingBox;
    Vector2 lastPos;

    BoxMinMaxSize boxConstraint;

    private void OnEnable() {
        if(boxConstraint != null) {
            boxConstraint.enabled = true;
        }
    }

    private void Start() {
        boxConstraint = GetComponentInChildren<BoxMinMaxSize>(true);
    }

    // Update is called once per frame
    void Update() {

        if (isMakingBox) {
            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                isMakingBox = false;
                boxConstraint.enabled = true;
            }
        }

        if (isMakingBox) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 delta = mousePos - lastPos;
            boxRT.SetBottom(boxRT.GetBottom() + delta.y);
            boxRT.SetRight(boxRT.GetRight() - delta.x);
            lastPos = mousePos;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame) {
            bool isValid = true;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            foreach (RectTransform rt in excludedRT) {
                if (rt.gameObject.activeInHierarchy) {
                    if (RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos)) {
                        isValid = false;
                    }
                    if (!isValid)
                        break;
                }
            }
            if (isValid) {
                isMakingBox = true;
                boxConstraint.enabled = false;
                lastPos = mousePos;
                boxRT.SetTop(Screen.height - mousePos.y);
                boxRT.SetLeft(mousePos.x);
                boxRT.SetBottom(mousePos.y);
                boxRT.SetRight(Screen.width - mousePos.x);

                boxRT.gameObject.SetActive(true);
            }
        }
    }
}
