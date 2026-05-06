using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using System.Text;
using System.IO;

[System.Serializable]
public struct VisibleStruct {
    public string id;
    public bool hasLabels;
    public string originalName;
};

[System.Serializable]
public struct CrossSectionsTags {
    public string tag;
    public bool enabled;
};

[System.Serializable]
public struct CrossSectionsStruct {
    public bool xEnabled, yEnabled, zEnabled;
    public bool xInverted, yInverted, zInverted;
    public float sliderValue;
    public List<CrossSectionsTags> tagsEnabled;
};

[System.Serializable]
public struct CamStruct {
    public Vector3 pos;
    public Quaternion rot;
    public float defaultDistance;
    public float distance;
}

[System.Serializable]
public struct NoteStruct {
    public Vector3 notePosition;
    public Vector3 gizmoPosition, gizmoNormal;
    public string text;
    public bool isOpened;
    public Vector2 size;
};

[System.Serializable]
public struct SaveStruct {
    public SpecieType specie;
    public List<VisibleStruct> visibleIds;
    public List<NoteStruct> notes;
    public CrossSectionsStruct crossSections;
    public CamStruct cam;
};

public class SaverLoader : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

    static SaveStruct currentSave;

    public string defaultFileName = "z-save";

    public GameObject loadingGO;

    public void OnSaveDown() {
#if UNITY_WEBGL && !UNITY_EDITOR
        Save();
#endif
    }

    public void OnSaveClick() {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        Save();
#endif
    }

    [ContextMenu("Save")]
    public void Save() {
        Dictionary<string, string> navidsMap = ParseNavidFile(true);

        if(navidsMap.Count == 0) {
            print("saving not available, no navid file found!");
        }

        List<VisibleStruct> visiblesNavids = new List<VisibleStruct>();
        foreach(BodyPartVisibility v in GlobalVariables.Instance.allVisibilityScripts) {
            if (v.isVisible && 
                    ((v.GetComponent<MeshFilter>() != null && v.GetComponent<MeshFilter>().sharedMesh != null && v.GetComponent<MeshFilter>().sharedMesh.vertexCount > 0) 
                    || v.GetComponent<Label>() != null)) {
                string origName = v.GetComponent<NameAndDescription>().originalName;
                string name = origName.Replace(".l", "").Replace(".r", "").Replace(".t", "").Replace(".s", "").Trim().ToLower();
                if (navidsMap.ContainsKey(name)) {
                    VisibleStruct vs;
                    vs.id = navidsMap[name];
                    vs.hasLabels = v.HasLabels() && v.labelsOn;
                    vs.originalName = origName;

                    visiblesNavids.Add(vs);
                }
                else if(v.tag == "Insertions") {
                    VisibleStruct vs;
                    vs.id = "insertions";
                    vs.hasLabels = false;
                    vs.originalName = origName;

                    visiblesNavids.Add(vs);
                }
            }
        }
        SaveStruct sStruct;
        sStruct.specie = GlobalVariables.Instance.GetCurrentSpecieSetting().type;
        sStruct.visibleIds = visiblesNavids;

        CrossSectionsStruct csStruct;
        csStruct.xEnabled = CrossSections.Instance.xPlane;
        csStruct.yEnabled = CrossSections.Instance.yPlane;
        csStruct.zEnabled = CrossSections.Instance.zPlane;
        csStruct.xInverted = CrossSections.Instance.ixPlane;
        csStruct.yInverted = CrossSections.Instance.iyPlane;
        csStruct.zInverted = CrossSections.Instance.izPlane;
        csStruct.sliderValue = CrossSections.Instance.frontalSlider.isActiveAndEnabled ? CrossSections.Instance.frontalSlider.value :
            (CrossSections.Instance.sagitalSlider.isActiveAndEnabled ? CrossSections.Instance.sagitalSlider.value :
            CrossSections.Instance.transversalSlider.value);

        List<CrossSectionsTags> tagsEnabled = new List<CrossSectionsTags>();
        List<string> tags = new List<string>{ "Skeleton", "Joints", "Lymph", "Muscles", "Fascia", "Arteries",
                                                "Veins", "Nervous", "Visceral", "BodyParts", "References" };
        foreach(string tag in tags) {
            CrossSectionsTags csTag;
            csTag.tag = tag;
            csTag.enabled = CrossSections.Instance.IsEnabledByTag(tag);
            tagsEnabled.Add(csTag);
        }
        csStruct.tagsEnabled = tagsEnabled;

        sStruct.crossSections = csStruct;

        CamStruct cStruct;
        cStruct.pos = CameraController.instance.transform.position;
        cStruct.rot = CameraController.instance.transform.rotation;
        cStruct.defaultDistance = CameraController.instance.defaulDistance;
        cStruct.distance = CameraController.instance.distance;
        sStruct.cam = cStruct;

        Note[] notes = FindObjectsOfType<Note>(true);
        List<NoteStruct> notesStruct = new List<NoteStruct>();
        foreach (Note note in notes) {
            NoteStruct ns;
            ns.notePosition = note.transform.position;
            ns.gizmoPosition = note.gizmo.hit.point;
            ns.gizmoNormal = note.gizmo.hit.normal;
            ns.text = note.tmpro_input.text;
            ns.isOpened = note.IsVisible();
            ns.size = note.GetComponent<RectTransform>().rect.size;
            notesStruct.Add(ns);
        }
        sStruct.notes = notesStruct;

        currentSave = sStruct;

        string jsonStr = JsonUtility.ToJson(sStruct, true);
        print(jsonStr);


#if UNITY_WEBGL && !UNITY_EDITOR
        var bytes = Encoding.UTF8.GetBytes(jsonStr);
        DownloadFile(gameObject.name, "OnFileDownload", defaultSaveName() + ".json", bytes, bytes.Length);
#else

        // Determine save path
        string filePath = DetermineFilePath();

        if (string.IsNullOrEmpty(filePath)) {
            Debug.Log("Saving cancelled by user");
        }
        else {
            // Save file
            File.WriteAllText(filePath, jsonStr);

            Debug.Log($"Saved: {filePath}");

            string[] pathTokens = filePath.Split(new string[] { "/" }, System.StringSplitOptions.RemoveEmptyEntries);
            string path = "";
            for (int i = 0; i < pathTokens.Length - 1; i++) {
                path += pathTokens[i] + "/";
            }
            PlayerPrefs.SetString("SavesPath", path);
        }
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // Called from browser
    //this is why the gameobject name needs to be unique!!
    public void OnFileDownload() {
        print("File Successfully Downloaded");
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(LoadAsync(url));
    }
#endif


    public void OnLoadDown() {
#if UNITY_WEBGL && !UNITY_EDITOR
        Load();
#endif
    }

    public void OnLoadClick() {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        Load();
#endif
    }

    [ContextMenu("Load")]
    public void Load() {
#if UNITY_WEBGL && !UNITY_EDITOR
        UploadFile(gameObject.name, "OnFileUpload", ".json", false);
#else
        string path = "";
        if (PlayerPrefs.HasKey("SavesPath")) {
            path = PlayerPrefs.GetString("SavesPath");
        }
        var paths = StandaloneFileBrowser.OpenFilePanel("Open file", path, "json", false);
        if (paths.Length > 0) {
            StartCoroutine(LoadAsync(new System.Uri(paths[0]).AbsoluteUri));
        }
        
#endif
    }


    public IEnumerator LoadAsync(string url) {

        if (string.IsNullOrEmpty(url)) {
            yield break;
        }

        int cullingMask = Camera.main.cullingMask;
        bool performedOK = false;
        try {
            Camera.main.cullingMask = LayerMask.GetMask("Loading");
            loadingGO.SetActive(true);

            var loader = new WWW(url);
            yield return loader;
            string jsonStr = loader.text;

            SaveStruct saving = JsonUtility.FromJson<SaveStruct>(jsonStr);

            if (saving.specie != GlobalVariables.Instance.GetCurrentSpecieSetting().type) {
                PopUpManagement.Instance.Show("This saving is for another specie: " + saving.specie.ToString());
                performedOK = true;
            }

            else {

                //first, destroy all notes
                Note[] notes = FindObjectsOfType<Note>(true);
                foreach(Note note in notes) {
                    note.Delete();
                }

                //manage visible objects
                if (saving.visibleIds.Count > 0) {
                    int count = 0;

                    Dictionary<string, string> navidsMap = ParseNavidFile();

                    Dictionary<BodyPartVisibility, VisibleStruct> found = new Dictionary<BodyPartVisibility, VisibleStruct>();
                    foreach (VisibleStruct vs in saving.visibleIds) {
                        if (navidsMap.ContainsKey(vs.id)) {
                            string searchName = navidsMap[vs.id];
                            BodyPartVisibility v = GlobalVariables.Instance.allVisibilityScripts.Find(delegate (BodyPartVisibility bpv)
                            {
                                string origName = bpv.GetComponent<NameAndDescription>().originalName;
                                return searchName == origName.Replace(".l", "").Replace(".r", "").Replace(".t", "").Replace(".s", "").Trim().ToLower()
                                    && vs.originalName == origName;
                            });
                            if (!found.ContainsKey(v)) {
                                found.Add(v, vs);
                            }
                        }
                        else if (vs.id == "insertions") {
                            BodyPartVisibility v = GlobalVariables.Instance.allVisibilityScripts.Find(delegate (BodyPartVisibility bpv)
                            {
                                return vs.originalName == bpv.GetComponent<NameAndDescription>().originalName;
                            });
                            if (!found.ContainsKey(v)) {
                                found.Add(v, vs);
                            }
                        }

                        count++;
                        if (count % 50 == 0) {
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    if (found.Count > 0) {
                        foreach (BodyPartVisibility v in GlobalVariables.Instance.allVisibilityScripts) {
                            bool show = found.ContainsKey(v);
                            v.gameObject.SetActive(show);
                            v.isVisible = show;
                            if (show) {
                                v.transform.SetActiveParentsRecursively(true, null);
                                if (found[v].hasLabels) {
                                    v.ShowLabels();
                                }
                            }
                        }
                    }
                    else {
                        print("found nothing!");
                    }

                }
                else {
                    print("no visible object to load!");
                }

                //reset stuff
                CommandController.Reset();

                //update lots of things internally!!
                SelectedObjectsManagement.Instance.GetActiveObjects();
                Lexicon.Instance.UpdateTreeViewCheckboxes();

                yield return new WaitForEndOfFrame();

                //Cross sections
                CrossSections.Instance.ResetAll();
                CrossPlanesGizmo.Instance.ResetAll();

                if (saving.crossSections.xEnabled || saving.crossSections.yEnabled || saving.crossSections.zEnabled
                    || saving.crossSections.xInverted || saving.crossSections.yInverted || saving.crossSections.zInverted) {
                    if (!CrossPlanesGizmo.Instance.opened) {
                        float time = CrossSections.Instance.planesOptionsPanel.durationOfAnimation;
                        CrossSections.Instance.planesOptionsPanel.durationOfAnimation = 0f;
                        CrossPlanesGizmo.Instance.OpenClosePlanesClick();
                        CrossSections.Instance.planesOptionsPanel.durationOfAnimation = time;
                    }

                    if (saving.crossSections.xEnabled || saving.crossSections.xInverted) {
                        CrossPlanesGizmo.Instance.XClick();
                        if (saving.crossSections.xInverted) {
                            CrossPlanesGizmo.Instance.InvertClick();
                        }
                        CrossSections.Instance.sagitalSlider.value = saving.crossSections.sliderValue;
                    }
                    else if (saving.crossSections.yEnabled || saving.crossSections.yInverted) {
                        CrossPlanesGizmo.Instance.YClick();
                        if (saving.crossSections.yInverted) {
                            CrossPlanesGizmo.Instance.InvertClick();
                        }
                        CrossSections.Instance.frontalSlider.value = saving.crossSections.sliderValue;
                    }
                    else if (saving.crossSections.zEnabled || saving.crossSections.zInverted) {
                        CrossPlanesGizmo.Instance.ZClick();
                        if (saving.crossSections.zInverted) {
                            CrossPlanesGizmo.Instance.InvertClick();
                        }
                        CrossSections.Instance.transversalSlider.value = saving.crossSections.sliderValue;
                    }

                    foreach (CrossSectionsTags csTag in saving.crossSections.tagsEnabled) {
                        bool isEnabled = CrossSections.Instance.IsEnabledByTag(csTag.tag);
                        if ((isEnabled && !csTag.enabled) || (!isEnabled && csTag.enabled)) {
                            switch (csTag.tag) {
                                case "Skeleton":
                                    CrossSections.Instance.skeletalToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Joints":
                                    CrossSections.Instance.jointsToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Lymph":
                                    CrossSections.Instance.lymphsToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Muscles":
                                    CrossSections.Instance.muscularToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Fascia":
                                    CrossSections.Instance.fasciaToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Arteries":
                                    CrossSections.Instance.arteriesToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Veins":
                                    CrossSections.Instance.veinsToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Nervous":
                                    CrossSections.Instance.nervousToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "Visceral":
                                    CrossSections.Instance.visceralToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "BodyParts":
                                    CrossSections.Instance.regionsToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                case "References":
                                    CrossSections.Instance.referencesToggle.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                //Camera
                CameraController.instance.transform.position = saving.cam.pos;
                CameraController.instance.transform.rotation = saving.cam.rot;
                if(saving.cam.defaultDistance > 0 && saving.cam.distance > 0) {
                    CameraController.instance.defaulDistance = saving.cam.defaultDistance;
                    CameraController.instance.distance = saving.cam.distance;
                }
                else {
                    CameraController.instance.CenterView(true);
                    throw new System.Exception();
                }

                //notes
                foreach(NoteStruct ns in saving.notes) {
                    Line3D line = Notes.instance.CreateLine(ns.gizmoPosition);
                    NoteGizmo gizmo = Instantiate(Notes.instance.gizmoPrefab);
                    RaycastHit hit = new RaycastHit();
                    hit.point = ns.gizmoPosition;
                    hit.normal = ns.gizmoNormal;
                    gizmo.hit = hit;
                    gizmo.placed = true;
                    gizmo.gameObject.SetActive(true);
                    //for now, do not handle linked body part since nothing is implemented concerning this feature!
                    Note note = Notes.instance.CreateNote(line, gizmo, ns.notePosition, null);
                    gizmo.note = note;
                    note.tmpro_input.text = ns.text;
                    note.GetComponent<RectTransform>().SetSize(ns.size);
                    if (ns.isOpened) {
                        note.Expand();
                    }
                    else {
                        note.Collapse();
                    }
                }

                performedOK = true;
            }
        }
        finally {
            //reset stuff
            loadingGO.SetActive(false);
            Camera.main.cullingMask = cullingMask;

            if (!performedOK) {
                PopUpManagement.Instance.Show("This file cannot be opened properly");
            }
        }
    }

    Dictionary<string, string> ParseNavidFile(bool reverse = false) {
        Dictionary<string, string> navidsMap = new Dictionary<string, string>();
        TextAsset navidFile = Resources.Load<TextAsset>(GlobalVariables.Instance.globalParent.name.Replace("@", "").ToLower() + "_navid");
        if (navidFile != null) {
            print("find navid file: " + navidFile.name);
            navidsMap = new Dictionary<string, string>();
            string[] lines = navidFile.text.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) {
                string[] tokens = line.Split(";", System.StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 2) {
                    print("Issue in navid line: " + line);
                }
                else {
                    if (reverse) {
                        if (!navidsMap.ContainsKey(tokens[1].Trim().ToLower())) {
                            navidsMap.Add(tokens[1].Trim().ToLower(), tokens[0]);
                        }
                    }
                    else {
                        if (!navidsMap.ContainsKey(tokens[0])) {
                            navidsMap.Add(tokens[0], tokens[1].Trim().ToLower());
                        }
                    }
                }
            }
            print("navid file parsed successfully!");
        }
        else {
            print("no navid file found!");
        }

        return navidsMap;
    }

    string defaultSaveName() {
        return $"{defaultFileName}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
    }

    // Method to determine save path
    private string DetermineFilePath() {
        // Generate default filename
        string defaultName = defaultSaveName();

        //SFB asset comes from this github: https://github.com/gkngkc/UnityStandaloneFileBrowser
        //error on build fixed copying two unity dlls Mono.Posix and Mono.WebBrowser into a plugins folder
        //fix found here: https://github.com/gkngkc/UnityStandaloneFileBrowser/issues/145
        string path = "";
        if (PlayerPrefs.HasKey("SavesPath")) {
            path = PlayerPrefs.GetString("SavesPath");
        }
        return StandaloneFileBrowser.SaveFilePanel("Save File", path, defaultName, "json");        
    }
}
