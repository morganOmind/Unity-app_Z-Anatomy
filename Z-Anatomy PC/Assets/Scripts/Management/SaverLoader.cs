using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VisibleStruct {
    public string id;
    public bool hasLabels;
    public string originalName;
};

[System.Serializable]
public struct CrossSectionsStruct {
    public bool xEnabled, yEnabled, zEnabled;
    public bool xInverted, yInverted, zInverted;
    public float sliderValue;
    public Dictionary<string, bool> tagsEnabled;
};

[System.Serializable]
public struct CamStruct {
    public Vector3 pos;
    public Quaternion rot;
    public float defaultDistance;
    public float distance;
}

[System.Serializable]
public struct SaveStruct {
    public SpecieType specie;
    public List<VisibleStruct> visibleIds;
    public List<Note> notes;    //TODO!!
    public CrossSectionsStruct crossSections;
    public CamStruct cam;
};

public class SaverLoader : MonoBehaviour
{

    static SaveStruct currentSave;

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
        sStruct.notes = null;

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

        Dictionary<string, bool> tagsEnabled = new Dictionary<string, bool>();
        List<string> tags = new List<string>{ "Skeleton", "Joints", "Lymph", "Muscles", "Fascia", "Arteries",
                                                "Veins", "Nervous", "Visceral", "BodyParts", "References" };
        foreach(string tag in tags) {
            tagsEnabled.Add(tag, CrossSections.Instance.IsEnabledByTag(tag));
        }
        csStruct.tagsEnabled = tagsEnabled;

        sStruct.crossSections = csStruct;

        CamStruct cStruct;
        cStruct.pos = CameraController.instance.transform.position;
        cStruct.rot = CameraController.instance.transform.rotation;
        cStruct.defaultDistance = CameraController.instance.defaulDistance;
        cStruct.distance = CameraController.instance.distance;
        sStruct.cam = cStruct;

        currentSave = sStruct;

        print(JsonUtility.ToJson(currentSave));
    }
    

    [ContextMenu("Load")]
    public void Load() {
        StartCoroutine(LoadAsync());
    }


    public IEnumerator LoadAsync() {

        if(currentSave.specie != GlobalVariables.Instance.GetCurrentSpecieSetting().type) {
            PopUpManagement.Instance.Show("This saving is for another specie: " + currentSave.specie.ToString());
            yield break;
        }

        if(currentSave.visibleIds.Count > 0) {
            Dictionary<string, string> navidsMap = ParseNavidFile();

            Dictionary<BodyPartVisibility, VisibleStruct> found = new Dictionary<BodyPartVisibility, VisibleStruct>();
            foreach(VisibleStruct vs in currentSave.visibleIds) {
                if (navidsMap.ContainsKey(vs.id)) {
                    string searchName = navidsMap[vs.id];
                    List<BodyPartVisibility> lv = GlobalVariables.Instance.allVisibilityScripts.FindAll(delegate (BodyPartVisibility bpv) 
                    {
                        string origName = bpv.GetComponent<NameAndDescription>().originalName;
                        return searchName == origName.Replace(".l", "").Replace(".r", "").Replace(".t", "").Replace(".s", "").Trim().ToLower()
                            && vs.originalName == origName;
                    });
                    foreach(BodyPartVisibility v in lv) {
                        if (!found.ContainsKey(v)) {
                            found.Add(v, vs);
                        }
                    }
                }
                else if(vs.id == "insertions") {
                    BodyPartVisibility v = GlobalVariables.Instance.allVisibilityScripts.Find(delegate (BodyPartVisibility bpv)
                    {
                        return vs.originalName == bpv.GetComponent<NameAndDescription>().originalName;
                    });
                    if (!found.ContainsKey(v)) {
                        found.Add(v, vs);
                    }
                }
            }

            if(found.Count > 0) {
                foreach(BodyPartVisibility v in GlobalVariables.Instance.allVisibilityScripts) {
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

        if(currentSave.crossSections.xEnabled || currentSave.crossSections.yEnabled || currentSave.crossSections.zEnabled
            || currentSave.crossSections.xInverted || currentSave.crossSections.yInverted || currentSave.crossSections.zInverted) {
            if (!CrossPlanesGizmo.Instance.opened) {
                float time = CrossSections.Instance.planesOptionsPanel.durationOfAnimation;
                CrossSections.Instance.planesOptionsPanel.durationOfAnimation = 0f;
                CrossPlanesGizmo.Instance.OpenClosePlanesClick();
                CrossSections.Instance.planesOptionsPanel.durationOfAnimation = time;
            }

            if (currentSave.crossSections.xEnabled || currentSave.crossSections.xInverted) {
                CrossPlanesGizmo.Instance.XClick();
                if (currentSave.crossSections.xInverted) {
                    CrossPlanesGizmo.Instance.InvertClick();
                }
                CrossSections.Instance.sagitalSlider.value = currentSave.crossSections.sliderValue;
            }
            else if (currentSave.crossSections.yEnabled || currentSave.crossSections.yInverted) {
                CrossPlanesGizmo.Instance.YClick();
                if (currentSave.crossSections.yInverted) {
                    CrossPlanesGizmo.Instance.InvertClick();
                }
                CrossSections.Instance.frontalSlider.value = currentSave.crossSections.sliderValue;
            }
            else if (currentSave.crossSections.zEnabled || currentSave.crossSections.zInverted) {
                CrossPlanesGizmo.Instance.ZClick();
                if (currentSave.crossSections.zInverted) {
                    CrossPlanesGizmo.Instance.InvertClick();
                }
                CrossSections.Instance.transversalSlider.value = currentSave.crossSections.sliderValue;
            }

            foreach (KeyValuePair<string, bool> pair in currentSave.crossSections.tagsEnabled) {
                bool isEnabled = CrossSections.Instance.IsEnabledByTag(pair.Key);
                if ((isEnabled && !pair.Value) || (!isEnabled && pair.Value)) {
                    switch (pair.Key) {
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
        CameraController.instance.transform.position = currentSave.cam.pos;
        CameraController.instance.transform.rotation = currentSave.cam.rot;
        CameraController.instance.defaulDistance = currentSave.cam.defaultDistance;
        CameraController.instance.distance = currentSave.cam.distance;
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
}
