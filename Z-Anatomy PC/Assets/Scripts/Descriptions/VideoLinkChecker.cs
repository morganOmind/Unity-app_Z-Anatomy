using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Text;

public class VideoLinkChecker : MonoBehaviour
{

    public static VideoLinkChecker instance;

    Vector2 mousePos;
    private TextMeshProUGUI descriptionTMPro;
    string word, contextWord;
    float wordBeginTime, notOnWordBeginTime;
    public float waitTime = 0.5f;

    Dictionary<List<string>, List<string>> videoLinksMap;
    Dictionary<List<List<string>>, List<string>> allNamesVideoLinksMap;
    List<string> notBodyPartsHasUrl;

    public RectTransform urlContextMenu;

    public Sprite youtubeIcon, otherUrlIcon;

    public Color urlAvailableColor;

    private void Start() {

        instance = this;

        descriptionTMPro = GetComponent<TextMeshProUGUI>();
        urlContextMenu.gameObject.SetActive(false);

        TextAsset videoLinksFile = Resources.Load<TextAsset>(GlobalVariables.Instance.globalParent.name.Replace("@", "").ToLower() + "_videoLinks");
        if (videoLinksFile != null) {
            print("find videoLinks file: " + videoLinksFile.name);

            videoLinksMap = new Dictionary<List<string>, List<string>>();

            string[] lines = videoLinksFile.text.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) {
                string[] tokens = line.Split(";", System.StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) {
                    print("Issue in videoLinks line: " + line);
                }

                List<string> names = new List<string>(tokens[0].Split("%", System.StringSplitOptions.RemoveEmptyEntries));
                for(int i=0; i<names.Count; i++) {
                    names[i] = names[i].ToLower();
                }

                List<string> urls = new List<string>();
                for(int i=1; i<tokens.Length; i++) {
                    urls.Add(tokens[i]);
                }
                videoLinksMap.Add(names, urls);
            }

            print("videoLinks file parsed successfully!");

            allNamesVideoLinksMap = new Dictionary<List<List<string>>, List<string>>();
            List<List<string>> allBodyPartsWithUrl = new List<List<string>>();

            foreach (var nameScript in GlobalVariables.Instance.allNameScripts) {
                List<string> allNames = new List<string>();

                string name = (nameScript.allNames == null || nameScript.allNames.Length == 0) ? nameScript.originalName : nameScript.allNames[GlobalVariables.Instance.GetCurrentSpecieSetting().descriptionLanguageIndexOverride];
                allNames.Add(name.Replace("(R)", "").Replace("(L)", "").Trim().ToLower());

                if (nameScript.HasSynonims(GlobalVariables.Instance.GetCurrentSpecieSetting().descriptionLanguageIndexOverride)) {
                    foreach (var synonym in nameScript.allSynonyms[GlobalVariables.Instance.GetCurrentSpecieSetting().descriptionLanguageIndexOverride]) {
                        allNames.Add(synonym.ToLower());
                    }
                }

                foreach(KeyValuePair<List<string>, List<string>> pairs in videoLinksMap) {
                    foreach(string n in allNames) {
                        if (pairs.Key.Contains(n)) {
                            //we have a match, this object has video links
                            //build map per language
                            //for now, this is not needed because descriptions are in only one language per specie
                            //but it will become interesting when we will have different description languages
                            //when it will be available, the update function should check the settings.languageIndex instead of the per-specie descriptionLanguageIndexOverride
                            List<List<string>> finalLists = new List<List<string>>();
                            string[] all = nameScript.allNames;
                            List<string[]> synonyms = nameScript.allSynonyms;

                            for(int i=0; i<all.Length; i++) {
                                List<string> list = new List<string>();
                                list.Add(all[i].ToLower().Trim());
                                if(synonyms != null && i < synonyms.Count && synonyms[i] != null && synonyms[i].Length > 0) {
                                    foreach(string s in synonyms[i]) {
                                        list.Add(s.ToLower().Trim());
                                    }
                                }
                                finalLists.Add(list);
                                if (i > allBodyPartsWithUrl.Count - 1) {
                                    allBodyPartsWithUrl.Add(list);
                                }
                                else {
                                    allBodyPartsWithUrl[i].AddRange(list);
                                }
                            }
                            allNamesVideoLinksMap.Add(finalLists, pairs.Value);
                        }
                    }
                }
            }

            //for now, we dont have translation of not-body parts, so consider only override description language
            notBodyPartsHasUrl = new List<string>();
            //check initial names with urls
            foreach (KeyValuePair<List<string>, List<string>> pairs in videoLinksMap) {
                foreach (string s in pairs.Key) {
                    if (!allBodyPartsWithUrl[GlobalVariables.Instance.GetCurrentSpecieSetting().descriptionLanguageIndexOverride].Contains(s)) {
                       notBodyPartsHasUrl.Add(s);
                       //print(s);
                    }
                }
            }

        }
        else {
            enabled = false;
        }
    }

    private void Update() {
        if (!string.IsNullOrEmpty(descriptionTMPro.text)) {
            int index = TMP_TextUtilities.FindIntersectingWord(descriptionTMPro, Mouse.current.position.ReadValue(), null);
            if (index != -1) {
                mousePos = Mouse.current.position.ReadValue();
                TMP_WordInfo wordInfo = descriptionTMPro.textInfo.wordInfo[index];
                string curWord = wordInfo.GetWord();

                if (curWord.Contains('’')) {
                    string[] tokens = curWord.Split('’', System.StringSplitOptions.RemoveEmptyEntries);
                    curWord = tokens[tokens.Length - 1];
                }
                curWord = curWord.ToLower().Trim();

                if(curWord != word || (!string.IsNullOrEmpty(contextWord) && curWord != contextWord)) {
                    wordBeginTime = Time.time;
                    checkCloseContext();
                }
                if(Time.time - wordBeginTime >= waitTime && !urlContextMenu.gameObject.activeSelf) {
                    List<string> urls = null;
                    foreach (KeyValuePair<List<List<string>>, List<string>> pair in allNamesVideoLinksMap) {
                        if (pair.Key[GlobalVariables.Instance.GetCurrentSpecieSetting().descriptionLanguageIndexOverride].Contains(curWord)) {
                            urls = pair.Value;
                        }
                        if (urls != null)
                            break;
                    }
                    if(urls == null) {
                        if (notBodyPartsHasUrl.Contains(curWord)) {
                            foreach(KeyValuePair<List<string>, List<string>> pair in videoLinksMap) {
                                if(pair.Key.Contains(curWord)){
                                    urls = pair.Value;
                                }
                                if (urls != null)
                                    break;
                            }
                        }
                    }
                    if(urls != null && urls.Count > 0) {
                        ShowUrlContextMenu(urls);
                        contextWord = curWord;
                    }
                }
                word = curWord;
            }
            else {
                word = "";
                checkCloseContext();
            }
        }
        else {
            word = "";
            urlContextMenu.gameObject.SetActive(false);
            contextWord = "";
        }

    }

    void checkCloseContext() {
        if (!urlContextMenu.gameObject.activeSelf)
            return;

        if (notOnWordBeginTime < 0f) {
            notOnWordBeginTime = Time.time;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(urlContextMenu, Mouse.current.position.ReadValue())) {
            notOnWordBeginTime = Time.time;
        }
        if (Time.time - notOnWordBeginTime > waitTime) {
            urlContextMenu.gameObject.SetActive(false);
            contextWord = "";
        }
    }

    void ShowUrlContextMenu(List<string> urls) {
        notOnWordBeginTime = -1f;
        urlContextMenu.gameObject.SetActive(true);
        Vector2 mousePos = Mouse.current.position.ReadValue();
        urlContextMenu.position = new Vector2(
                Mathf.Clamp(mousePos.x + urlContextMenu.GetWidth() / 2, urlContextMenu.GetWidth() * 0.75f, Screen.width - urlContextMenu.GetWidth() * 0.75f),
                Mathf.Clamp(mousePos.y - urlContextMenu.GetHeight() / 2, urlContextMenu.GetHeight() * 0.75f, Screen.height - urlContextMenu.GetHeight() * 0.75f)
                );

        MakeUrlButton(urlContextMenu.transform.GetChild(0).GetComponentInChildren<UrlButton>(), urls[0]);
        for(int i =1; i<urlContextMenu.childCount; i++) {
            GameObject.Destroy(urlContextMenu.GetChild(i).gameObject);
        }
        for(int i = 1; i<urls.Count; i++) {
            GameObject button = GameObject.Instantiate(urlContextMenu.transform.GetChild(0).gameObject, urlContextMenu);
            MakeUrlButton(button.GetComponentInChildren<UrlButton>(), urls[i]);
        }
    }

    void MakeUrlButton(UrlButton button, string url) {
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = url;
        button.icon.sprite = url.Contains("youtu") ? youtubeIcon : otherUrlIcon;
        button.url = url;
    }

    public string HightlightText(string desc) {
        foreach(string s in notBodyPartsHasUrl) {
            desc = HighlightVariations(desc, s);
            desc = HighlightVariations(desc, s[0].ToString() + s.Substring(1));
        }
        return desc;
    }

    string HighlightVariations(string desc, string name) {
        desc = HightlightWord(desc, name + " ");
        desc = HightlightWord(desc, name + ",");
        desc = HightlightWord(desc, name + ".");
        desc = HightlightWord(desc, name + ";");
        desc = HightlightWord(desc, name + ":");
        return desc;
    }

    string HightlightWord(string desc, string name) {
        return desc.Replace(name, "<color=#" + ColorUtility.ToHtmlStringRGB(urlAvailableColor) + "><b>" + name + "</b></color>");
    }
}
