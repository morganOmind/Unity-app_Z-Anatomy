using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropDownLanguageSetting : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData) {
        Toggle[] toggles = transform.Find("Dropdown List").Find("Viewport").Find("Content").GetComponentsInChildren<Toggle>();
        List<int> availableLanguages = new List<int>(GlobalVariables.Instance.GetCurrentSpecieSetting().availableLanguages);
        for (int i = 0; i < toggles.Length; i++) {
            toggles[i].interactable = availableLanguages.IndexOf(i) != -1;
        }
    }
}
