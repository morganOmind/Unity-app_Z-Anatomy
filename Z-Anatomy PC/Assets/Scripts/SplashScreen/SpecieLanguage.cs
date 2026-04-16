using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpecieLanguage : MonoBehaviour, IPointerClickHandler {

    public LanguageMgr changeLanguage;

    public void OnPointerClick(PointerEventData eventData) {
        Toggle[] toggles = transform.Find("Dropdown List").Find("Viewport").Find("Content").GetComponentsInChildren<Toggle>();
        int language = changeLanguage.GetComponent<TMPro.TMP_Dropdown>().value;
        for (int i = 0; i < toggles.Length; i++) {
            SpecieTranslation translation = changeLanguage.speciesTranslations[i];
            string text = translation.english;
            switch (language) {
                case 0:
                    break;
                case 1:
                    text = translation.latin;
                    break;
                case 2:
                    text = translation.french;
                    break;
                case 3:
                    text = translation.espanol;
                    break;
                case 4:
                    text = translation.portugues;
                    break;
                default: break;
            }
            toggles[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
        }
    }
}