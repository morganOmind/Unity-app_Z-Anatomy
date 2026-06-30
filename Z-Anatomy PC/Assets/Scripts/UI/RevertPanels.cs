using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RevertPanels : MonoBehaviour
{
    PanelsManagement mgr;

    public List<RectTransform> verticalPanels, otherPanels;
    public RectTransform verticalGrid;
    public List<GameObject> leftDrags, rightDrags;
    public GameObject goToLeftButton, goToRightButton;
    public RectTransform gizmoParent;

    [Header("OnLeftNoVerticalPanels")]
    public List<RectTransform> toolsRT;

    public static bool isOnLeft = false;

    public static bool toolsPanelOnLeft = false;

    private void Start() {
        mgr = PanelsManagement.instance;
        foreach(GameObject rd in rightDrags) {
            rd.SetActive(false);
        }
    }

    private void Update() {
        bool hasPanel = PanelsManagement.instance.hasSomePanelOpened();
        goToLeftButton.SetActive(hasPanel && !isOnLeft);
        goToRightButton.SetActive(hasPanel && isOnLeft);

        if(!toolsPanelOnLeft && isOnLeft && !PanelsManagement.instance.hasSomePanelOpened()) {
            toolsPanelOnLeft = true;
            foreach(RectTransform rt in toolsRT) {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - (verticalGrid.GetWidth() + 25f), rt.anchoredPosition.y);
            }
            foreach(RectTransform rt in verticalPanels) {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + toolsRT[0].GetWidth() + 25f, rt.anchoredPosition.y);
            }
        }

        if(toolsPanelOnLeft && isOnLeft && PanelsManagement.instance.hasSomePanelOpened()) {
            toolsPanelOnLeft = false;
            foreach (RectTransform rt in toolsRT) {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + verticalGrid.GetWidth() + 25f, rt.anchoredPosition.y);
            }
            foreach (RectTransform rt in verticalPanels) {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x - (toolsRT[0].GetWidth() + 25f), rt.anchoredPosition.y);
            }
        }
    }

    public void GoTo(bool left) {
        print("revert panels to " + (left ? "left" : "right"));

        verticalGrid.anchorMax = new Vector2(left ? 0f : 1f, verticalGrid.anchorMax.y);
        verticalGrid.pivot = new Vector2(left ? 0f : 1f, verticalGrid.pivot.y);

        mgr.ReinitPanelWidth();
        isOnLeft = left;    //very important to set it exactly here!!

        //reset vertical panels setup to avoid weird placement issues (the whole app UI should be refactored to be properly setup from the start!!)

        float animDuration = mgr.lex.durationOfAnimation;
        mgr.lex.durationOfAnimation = 0f;
        mgr.desc.durationOfAnimation = 0f;
        mgr.settings.durationOfAnimation = 0f;
        mgr.help.durationOfAnimation = 0f;

        if (mgr.settings.isExpanded) {
            mgr.settings.Collapse();
            mgr.settingsTab.Deselect();
            mgr.settingsOnScreen = false;
        }
        if (mgr.help.isExpanded) {
            mgr.help.Collapse();
            mgr.helpTab.Deselect();
            mgr.helpOnScreen = false;
        }

        if (!mgr.lexOnScreen)
            mgr.ShowLexicon();
        if (!mgr.descOnScreen)
            mgr.ShowDescription();

        mgr.lex.durationOfAnimation = animDuration;
        mgr.desc.durationOfAnimation = animDuration;
        mgr.settings.durationOfAnimation = animDuration;
        mgr.help.durationOfAnimation = animDuration;

        mgr.SomeVerticalPanelOpened();

        foreach (RectTransform rt in verticalPanels) {
            float xAnchor = left ? 0f : 1f;
            rt.anchorMin = new Vector2(xAnchor, rt.anchorMin.y);
            rt.anchorMax = new Vector2(xAnchor, rt.anchorMax.y);

            if(rt.pivot.x == 0f || rt.pivot.x == 1f) {
                rt.pivot = new Vector2(xAnchor, rt.pivot.y);
            }

            rt.anchoredPosition = new Vector2(-rt.anchoredPosition.x, rt.anchoredPosition.y);
        }

        reverseHide(mgr.lexOnScreen, mgr.lex, isOnLeft ? 0f : 287.5f);
        reverseHide(mgr.descOnScreen, mgr.desc, isOnLeft ? 0f : 287.5f);
        reverseHide(mgr.helpOnScreen, mgr.help, 0f);
        reverseHide(mgr.settingsOnScreen, mgr.settings, 0f);

        foreach(RectTransform rt in otherPanels) {
            float offsetWidth = mgr.GetPanelWidth() + goToLeftButton.GetComponent<RectTransform>().rect.size.x;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + ((left ? 1f : -1f) * offsetWidth), rt.anchoredPosition.y);

            ExpandCollapseUI collapseUI = rt.GetComponent<ExpandCollapseUI>();
            if (collapseUI != null) {
                collapseUI.expandedPosition = new Vector2(rt.anchoredPosition.x, collapseUI.expandedPosition.y);
                collapseUI.collapasedPosition = new Vector2(rt.anchoredPosition.x, collapseUI.collapasedPosition.y);
            }
        }

        foreach(GameObject ld in leftDrags) {
            ld.SetActive(!left);
        }
        foreach (GameObject rd in rightDrags) {
            rd.SetActive(left);
        }

        for(int i=0; i<gizmoParent.childCount; i++) {
            RectTransform rt = gizmoParent.GetChild(i).GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(isOnLeft ? 0.9f : 0f, rt.anchorMin.y);
            rt.anchorMax = new Vector2(isOnLeft ? 0.9f : 0f, rt.anchorMax.y);
        }
    }

    void reverseHide(bool onScreen, ExpandCollapseUI ui, float expandedX) {
        ui.SetCollapsedPositionX((int)-ui.collapasedPosition.x);
        ui.expandedPosition = new Vector2(expandedX, ui.expandedPosition.y);

        RectTransform rt = ui.GetComponent<RectTransform>();
        
        rt.anchorMin = new Vector2(isOnLeft ? 0f : 1f, rt.anchorMin.y);
        rt.anchorMax = new Vector2(isOnLeft ? 0f : 1f, rt.anchorMax.y);
        rt.pivot = new Vector2(isOnLeft ? 0f : 1f, rt.pivot.y);

        if (!onScreen) {
            rt.anchoredPosition = ui.collapasedPosition; //new Vector2(-rt.anchoredPosition.x, rt.anchoredPosition.y);
        }
        else {
            rt.anchoredPosition = new Vector2(0f, rt.anchoredPosition.y);
        }

    }
}
