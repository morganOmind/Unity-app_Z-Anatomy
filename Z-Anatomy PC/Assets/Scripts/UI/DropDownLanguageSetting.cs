using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownLanguageSetting : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        Toggle[] toggles = transform.Find("Dropdown List").Find("Viewport").Find("Content").GetComponentsInChildren<Toggle>();
        List<int> availableLanguages = new List<int>(GlobalVariables.Instance.GetCurrentSpecieSetting().availableLanguages);
        float offset = 0f;
        for (int i = 0; i < toggles.Length; i++) {
            bool enable = availableLanguages.IndexOf(i) != -1;
            toggles[i].gameObject.SetActive(enable);
            if (!enable) {
                //move up the next ones and add their heights to the scroll view offset
                for(int j=i+1; j<toggles.Length; j++) {
                    RectTransform rect = toggles[j].GetComponent<RectTransform>();
                    Vector2 pos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(pos.x, pos.y + toggles[j - 1].GetComponent<RectTransform>().rect.size.y);
                }
                offset += toggles[i].GetComponent<RectTransform>().rect.size.y;
            }
        }
        if(offset > 0f) {
            RectTransform rect = transform.Find("Dropdown List").GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            rect.sizeDelta = new Vector2(size.x, size.y - offset);
        }
    }
}
