using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpecieTranslation{
    public string english, latin, french, espanol, portugues;
};


public class LanguageMgr : MonoBehaviour
{
    public List<SpecieTranslation> speciesTranslations;

    public TMPro.TextMeshProUGUI specieLabel;

    int currentLanguage = 0;

    private void Start() {
        //if we dont start with params in url
        if (string.IsNullOrEmpty(UrlParser.openNavid)) {
            //set default specie
            OnSpecieChange(0);
            //set language
            int language = 0;
            if (PlayerPrefs.HasKey("Language"))
                language = PlayerPrefs.GetInt("Language");
            else {
                language = Settings.GetSystemLanguage();
            }
            GetComponent<TMPro.TMP_Dropdown>().SetValueWithoutNotify(language);
            applyLanguageChange(language);
        }
    }

    public void OnLanguageChange(int value) {
        applyLanguageChange(value);
        PlayerPrefs.SetInt("Language", value);
    }

    void applyLanguageChange(int value) {
        currentLanguage = value;

        foreach (MultilanguageText t in FindObjectsOfType<MultilanguageText>(true)) {
            switch (value) {
                case 0:
                    t.TranslateTo(SystemLanguage.English);
                    break;
                case 1:
                    t.TranslateTo(SystemLanguage.English);
                    break;
                case 2:
                    t.TranslateTo(SystemLanguage.French);
                    break;
                case 3:
                    t.TranslateTo(SystemLanguage.Spanish);
                    break;
                case 4:
                    t.TranslateTo(SystemLanguage.Portuguese);
                    break;
                default: break;
            }
        }

        int translationIndex = speciesTranslations.FindIndex(s => 
        s.english == specieLabel.text 
        || s.french == specieLabel.text 
        || s.latin == specieLabel.text
        || s.espanol == specieLabel.text
        || s.portugues == specieLabel.text);

        OnSpecieChange(translationIndex);
    }

    public void OnSpecieChange(int value) {
        string specieText = speciesTranslations[value].english;
        switch (currentLanguage) {
            case 0:
                break;
            case 1:
                specieText = speciesTranslations[value].latin;
                break;
            case 2:
                specieText = speciesTranslations[value].french;
                break;
            case 3:
                specieText = speciesTranslations[value].espanol;
                break;
            case 4:
                specieText = speciesTranslations[value].portugues;
                break;
            default: break;
        }
        specieLabel.text = specieText;
        GlobalVariables.specieType = (SpecieType)(value + 1);   //+1 because 0 is UNKNOWN!
    }
}
