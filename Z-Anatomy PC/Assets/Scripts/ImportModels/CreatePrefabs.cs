using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using TMPro;

public class CreatePrefabs : MonoBehaviour
{
#if UNITY_EDITOR

    [MenuItem("Utils/Delete PlayerPrefs")]
    static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }


    [MenuItem("Prefabs/Delete global labels FIRST STEP (Select all) -- LEGACY process")]
    static void DeleteGlobalLabels()
    {
        DeleteGlobalLabels(Selection.gameObjects);
    }

    [MenuItem("Prefabs/Process LEGACY human Model SECOND STEP (Select only one collection's parent, and TAG IT before processing)")]
    static void ProcessLegacyHumanModel()
    {
        ProcessInternal(true);   
    }

    [MenuItem("Prefabs/Process Model SECOND STEP (Select only one collection's parent, and TAG IT before processing)")]
    static void ProcessModel() {
        ProcessInternal(false);
    }

    static Dictionary<string, string> navidsMap;

    static void ProcessInternal(bool isLegacyHumanModel) {
        try {

            navidsMap = null;
            TextAsset navidFile = Resources.Load<TextAsset>(Selection.activeGameObject.transform.parent.name.Replace("@", "").ToLower() + "_navid");
            if(navidFile != null) {
                print("find navid file: " + navidFile.name);
                navidsMap = new Dictionary<string, string>();
                string[] lines = navidFile.text.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    string[] tokens = line.Split(";", System.StringSplitOptions.RemoveEmptyEntries);
                    if(tokens.Length != 2) {
                        print("Issue in navid line: " + line);
                    }
                    else {
                        navidsMap.Add(tokens[0], tokens[1]);
                    }
                }
                print("navid file parsed successfully!");
            }

            if(PrefabUtility.IsPartOfAnyPrefab(Selection.activeGameObject)) {
                PrefabUtility.UnpackPrefabInstance(Selection.activeGameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            AddScriptAndMesh(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList());
            if (!isLegacyHumanModel) {
                ReparentLines(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList());
            }
            CreateLabels(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList(), isLegacyHumanModel);
            SetLayer(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList());
            SetTag(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList(), Selection.activeGameObject.tag);
            if(navidsMap != null) {
                Rename(Selection.activeGameObject.GetComponentsInChildren<Transform>(true).ToList());
            }
        }
        catch (System.Exception) {
            throw;
        }
    }

    //This method deltes the lines of the global groups
    private static void DeleteGlobalLabels(GameObject[] selectedObjects)
    {
        foreach (var obj in selectedObjects)
        {
            if ((obj.name.Contains(".j") || obj.name.Contains(".i")) && obj.transform.parent.name.Contains(".g"))
            {
                DestroyImmediate(obj.gameObject);
            }
        }
    }

     private static void AddScriptAndMesh(List<Transform> gameObjects)
     {
         string actual = "";
         try
         {
             foreach (Transform child in gameObjects)
             {
                 actual = child.name;
                if(!child.name.Contains(".j") && !child.name.Contains(".i"))
                {
                    child.gameObject.AddComponent<NameAndDescription>();
                    child.gameObject.AddComponent<BodyPartVisibility>();
                }
                if (!child.name.Contains(".j") && !child.name.Contains(".i" )&& !child.name.Contains(".t") && !child.name.Contains(".s") && child.GetComponent<MeshRenderer>() != null)
                 {
                    if(child.GetComponent<MeshFilter>().sharedMesh.vertexCount == 0) {
                        Component.DestroyImmediate(child.GetComponent<MeshRenderer>());
                        Component.DestroyImmediate(child.GetComponent<MeshFilter>());
                    }
                    else {
                        TangibleBodyPart script = child.gameObject.AddComponent<TangibleBodyPart>();
                        if (script == null)
                            continue;
                        child.gameObject.AddComponent<MeshCollider>();
                    }
                }
             }
         }
         catch (System.Exception e)
         {
             Debug.Log("Error adding script and mesh to " + actual + ": " + e.Message);
         }
     }

    private static void SetLayer(List<Transform> gameObjects)
    {
        foreach (var obj in gameObjects)
        {
            obj.gameObject.layer = LayerMask.NameToLayer("Body");

        }
    }

    static bool isLineLimit(GameObject obj) {
        return obj.name == "minPoint" || obj.name == "maxPoint";
    }

    private static void SetTag(List<Transform> gameObjects, string tag) {
        foreach (var obj in gameObjects) {
            string[] tokens = obj.gameObject.name.Split(".", System.StringSplitOptions.RemoveEmptyEntries);
            string suffix = tokens[tokens.Length - 1];
            if(suffix.StartsWith("o") || suffix.StartsWith("e")) {
                obj.gameObject.tag = "Insertions";
            }
            else {
                if(!isLineLimit(obj.gameObject)) {
                    obj.gameObject.tag = tag;
                }
            }
        }
    }

    private static void Rename(List<Transform> gameObjects) {
        foreach (var obj in gameObjects) {
            if(obj.gameObject != Selection.activeGameObject && !isLineLimit(obj.gameObject)) {
                string[] tokens = obj.name.Split(".", System.StringSplitOptions.RemoveEmptyEntries);
                if (navidsMap.ContainsKey(tokens[0])) {
                    obj.name = navidsMap[tokens[0]];
                    for (int i = 1; i < tokens.Length; i++) {
                        obj.name += ("." + tokens[i]);
                    }
                }
                else {
                    Debug.LogError("Navid " + tokens[0] + " not present in the map");
                }
            }
        }
    }

    private static void CreateLabels(List<Transform> gameObjects, bool isHuman)
    {
        foreach (Transform child in gameObjects)
        {
            if (child.name.Contains(".j") || child.name.Contains(".i"))
            {
                CreateLinePoints(child.gameObject);
                Line script = child.gameObject.AddComponent<Line>();
                script.lineMaterial = (Material)Resources.Load("LineMaterial", typeof(Material));
                if (isHuman) {
                    script.transform.parent = script.transform.parent.parent;
                }
                //script.gameObject.SetActive(false);
            }
            else if (child.name.Contains(".t") || child.name.Contains(".s"))
            {
                Label script = child.gameObject.AddComponent<Label>();
                //child.gameObject.AddComponent<TextMeshPro>();
                //script.labelMaterial = (Material)Resources.Load("LabelMaterial", typeof(Material));
                if (!isHuman) {
                    script.transform.parent = script.transform.parent.parent;
                }
            }
        }
    }

    //manage sublabels/lines: reparent lines that are parented to a line
    //lines of sublabels are reparented to their associated parent's label instead of line to manage the lexicon hierarchy properly
    static void ReparentLines(List<Transform> gameObjects) {
        List<Transform> labels = gameObjects.FindAll(go => go.name.EndsWith(".s") || go.name.EndsWith(".t"));
        foreach (Transform child in gameObjects) {
            if ((child.name.EndsWith(".i") || child.name.EndsWith(".j")) 
                && (child.parent.name.EndsWith(".i") || child.parent.name.EndsWith(".j")) )
                {

                string parentLabelName = child.parent.name.Replace(".i", ".s").Replace(".j", ".t");
                Transform parentLabel = labels.Find(t => t.name == parentLabelName);

                if(parentLabel == null) {
                    print("Error reparenting " + child.name);
                }
                else {
                    child.parent = parentLabel;
                }
            }
        }
    }

     private static void CreateLinePoints(GameObject gameObject)
     {
         try
         {
             string name = gameObject.name;
             Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
             if (mesh.vertices.Length == 0) {
                print(gameObject.name + " mesh has no vertices");
                return;
            }
            Vector3 min = mesh.vertices[0];
            Vector3 max;
            if(mesh.vertices.Length >= 8) {
                max = mesh.vertices[7];
            }
            else {
                max = mesh.vertices[mesh.vertices.Length - 1];
            }

            GameObject minPoint = new GameObject();
             minPoint.name = "minPoint";
             minPoint.transform.parent = gameObject.transform;
             minPoint.transform.localPosition = min;

             GameObject maxPoint = new GameObject();
             maxPoint.name = "maxPoint";
             maxPoint.transform.parent = gameObject.transform;
             maxPoint.transform.localPosition = max;

             DestroyImmediate(gameObject.GetComponent<MeshFilter>());
             DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
         }
         catch (System.Exception e)
         {
             Debug.Log("Error creating line points of " + gameObject.name + ": " + e.Message);
         }
     }

#endif

}
