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
public struct SaveStruct {
    public List<VisibleStruct> visibleIds;
    public List<Note> notes;    //TODO!!
    //add specie, cross sections states, camera transform
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
            if (v.isVisible) {
                string origName = v.GetComponent<NameAndDescription>().originalName;
                string name = origName.Replace(".l", "").Replace(".r", "").Replace(".t", "").Replace(".s", "").Trim().ToLower();
                if (navidsMap.ContainsKey(name)) {
                    VisibleStruct vs;
                    vs.id = navidsMap[name];
                    vs.hasLabels = v.HasLabels() && v.labelsOn;
                    vs.originalName = origName;

                    visiblesNavids.Add(vs);
                }
            }
        }
        SaveStruct sStruct;
        sStruct.visibleIds = visiblesNavids;
        sStruct.notes = null;

        currentSave = sStruct;

        print(JsonUtility.ToJson(currentSave));
    }

    [ContextMenu("Load")]
    public void Load() {
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
                CameraController.instance.CenterView(true);
                CommandController.Reset();
            }
            else {
                print("found nothing!");
            }

        }
        else {
            print("nothing to load!");
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
}
