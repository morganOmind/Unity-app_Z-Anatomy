using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum SpecieType {
    Unknown, Man, Cat
};

[System.Serializable]
public struct SpecieSetting {
    public SpecieType type;
    public GameObject globalParent;
    public TextAsset translations;
    public int[] availableLanguages;
    public SystemLanguage descriptionLanguageOverride;
    public int descriptionLanguageIndexOverride;
    public int initialNameIndexInTranslationFile;
    public float camDefaultDistance;
    public float camMaxDistance;
    public TextAsset[] descriptions;
    public Vector2 sagitalLimits, coronalLimits, transversalLimits;
    public float crossSectionLineWidth;
    public SpecieLayers layers;
};

[System.Serializable]
public struct SpecieLayers {
    public TextAsset[] bonesLayers;
    public TextAsset[] ligamentsLayers;
    public TextAsset[] muscularLayers;
    public TextAsset[] arteriesLayers;
    public TextAsset[] veinsLayers;
    public TextAsset[] lymphsLayers;
    public TextAsset[] fasciaLayers;
    public TextAsset[] nervesLayers;
    public TextAsset[] visceralLayers;
    public TextAsset[] refsLayers;
    public TextAsset[] skinLayers;
};

public class GlobalVariables : MonoBehaviour
{
    [HideInInspector]
    public static GlobalVariables Instance;

    public SpecieType editorSpecieType = SpecieType.Man;
    public static SpecieType specieType = SpecieType.Unknown;

    public List<SpecieSetting> speciesSettings;

    [SerializeField]
    private Color _highligthColor;
    [SerializeField]
    private Color _secondaryColor;
    [SerializeField]
    private Color _surfaceColor;
    [SerializeField]
    private Color _onSurfaceColor;
    [SerializeField]
    private Color _backgroundColor;
    [SerializeField]
    private Color _iconColor;
    [SerializeField]
    private Color _disabledIconColor;
    [SerializeField]
    private Color _taskBarColor;

    public float labelFontSize;
    public float titleLabelFontSize;
    public float lineSize;

    public static Color HighligthColor;
    public static Color SecondaryColor;
    public static Color SurfaceColor;
    public static Color OnSurfaceColor;
    public static Color BackgroundColor;
    public static Color IconColor;
    public static Color DisabledIconColor;
    public static Color TaskBarColor;

    public bool refresh;

    public GameObject globalParent;
    [HideInInspector]
    public List<NameAndDescription> allNameScripts;
    [HideInInspector]
    public List<MeshRenderer> allBodyPartRenderers;
    [HideInInspector]
    public List<BodyPartVisibility> allVisibilityScripts;
    [HideInInspector]
    public List<TangibleBodyPart> allBodyParts;
    [HideInInspector]
    public List<GameObject> bodySections;

    [HideInInspector]
    public List<TangibleBodyPart> bones;
    [HideInInspector]
    public List<TangibleBodyPart> insertions;
    [HideInInspector]
    public Dictionary<string, TangibleBodyPart> insertionsDictionary = new Dictionary<string, TangibleBodyPart>();
    [HideInInspector]
    public Dictionary<string, TangibleBodyPart> musclesDictionary = new Dictionary<string, TangibleBodyPart>();
    [HideInInspector]
    public List<TangibleBodyPart> joints;
    [HideInInspector]
    public List<TangibleBodyPart> muscles;
    [HideInInspector]
    public List<TangibleBodyPart> lymphs;
    [HideInInspector]
    public List<TangibleBodyPart> arteries;
    [HideInInspector]
    public List<TangibleBodyPart> veins;
    [HideInInspector]
    public List<TangibleBodyPart> nerves;
    [HideInInspector]
    public List<TangibleBodyPart> viscera;
    [HideInInspector]
    public List<TangibleBodyPart> regions;
    [HideInInspector]
    public List<TangibleBodyPart> references;

    [Header("Loading specie")]
    public GameObject loadingGO;
    public RectTransform speciesChoiceRoot;
    public Image specieImage;
    public Transform canvasesRoot;

    private void Awake()
    {
        Instance = this;
        
        Build();

#if UNITY_EDITOR
        if(specieType == SpecieType.Unknown) {
            specieType = editorSpecieType;
        }
#endif

        print("Loading specie type: " + GetCurrentSpecieSetting().type.ToString());
        globalParent = GetCurrentSpecieSetting().globalParent;

        allNameScripts = globalParent.GetComponentsInChildren<NameAndDescription>(true).ToList();
        allBodyPartRenderers = globalParent.GetComponentsInChildren<MeshRenderer>(true).Where(it => it.GetComponent<Label>() == null && it.GetComponent<Line>() == null && !it.gameObject.name.Contains(".g")).ToList();
        allVisibilityScripts = globalParent.GetComponentsInChildren<BodyPartVisibility>(true).ToList();
        allBodyParts = globalParent.GetComponentsInChildren<TangibleBodyPart>(true).ToList();

        bones = allBodyParts.Where(it => it.CompareTag("Skeleton")).ToList();
        insertions = allBodyParts.Where(it => it.CompareTag("Insertions")).ToList();
        joints = allBodyParts.Where(it => it.CompareTag("Joints")).ToList();
        muscles = allBodyParts.Where(it => it.CompareTag("Muscles")).ToList();
        lymphs = allBodyParts.Where(it => it.CompareTag("Lymph")).ToList();
        arteries = allBodyParts.Where(it => it.CompareTag("Arteries")).ToList();
        veins = allBodyParts.Where(it => it.CompareTag("Veins")).ToList();
        nerves = allBodyParts.Where(it => it.CompareTag("Nervous")).ToList();
        viscera = allBodyParts.Where(it => it.CompareTag("Visceral")).ToList();
        regions = allBodyParts.Where(it => it.CompareTag("BodyParts")).ToList();
        references = allBodyParts.Where(it => it.CompareTag("References")).ToList();
        

        foreach (Transform section in globalParent.transform)
            bodySections.Add(section.gameObject);

        SetSpecie(specieType);
    }


    private void Start()
    {
        foreach (var insertion in insertions)
            insertionsDictionary.Add(insertion.nameScript.originalName, insertion);

        foreach (var muscle in muscles) {
            if (muscle.nameScript == null) {
                print(muscle.name + " has no namescript!");
            }
            else {
                musclesDictionary.Add(muscle.nameScript.originalName, muscle);
            }
        }

        StartCoroutine(SanityCheck());
        StartCoroutine(UrlNavidSelection());
    }

    private void OnValidate()
    {
        if(Instance == null)
            Instance = this;
        if (refresh)
        {
            refresh = false;
            Build();
        }
    }

    public void OnChangeSpecie(int type) {
        SpecieType specie = (SpecieType)type;
        if(specieType != specie) {
            specieType = specie;
            StartCoroutine(changeSpecieAsync());
        }
    }

    IEnumerator changeSpecieAsync() {
        Camera.main.cullingMask = LayerMask.GetMask("Loading");
        loadingGO.SetActive(true);
        globalParent.SetActive(false);
        for(int i=0; i<canvasesRoot.childCount; i++) {
            if(canvasesRoot.GetChild(i).gameObject != loadingGO) {
                canvasesRoot.GetChild(i).gameObject.SetActive(false);
            }
        }
        yield return new WaitForEndOfFrame();
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    void SetSpecie(SpecieType type) {
        specieType = type;
        foreach(SpecieSetting setting in speciesSettings) {
            if(setting.type == type) {
                globalParent = setting.globalParent;
                if(setting.translations != null && NamesManagement.Instance != null) {
                    NamesManagement.Instance.translations = setting.translations;
                }
                if(setting.descriptions != null && ReadLocalDefinitions.Instance != null) {
                    ReadLocalDefinitions.Instance.SetDescriptions(setting.descriptions);
                }
                if(CrossSections.Instance != null) {
                    CrossSections.Instance.SetSlidersLimits();
                }
                if(Layers.Instance != null) {
                    Layers.Instance.SetLayers(setting.layers);
                }
                globalParent.SetActive(true);

                //set camera params
                CameraController camCtrl = Camera.main.GetComponent<CameraController>();
                camCtrl.target = globalParent;
                camCtrl.defaultCenter = globalParent.transform.Find("DEFAULTCENTER").gameObject;
                camCtrl.defaulDistance = setting.camDefaultDistance;
                CameraController.MAX_DISTANCE = setting.camMaxDistance;
            }
            else {
                setting.globalParent.SetActive(false);
            }
        }
        ToggleChangeColor[] toggles = speciesChoiceRoot.GetComponentsInChildren<ToggleChangeColor>(true);
        for(int i=0; i<toggles.Length; i++) {
            bool isActive = ((int)type - 1) == i;
            if((isActive && !toggles[i].pressed) || (!isActive && toggles[i].pressed)){
                toggles[i].ChangeState();
            }
            if (isActive) {
                specieImage.sprite = toggles[i].transform.Find("Icon").GetComponent<Image>().sprite;
            }
        }

    }

    public SpecieSetting GetSpecieSetting(SpecieType type) {
        foreach (SpecieSetting setting in speciesSettings) {
            if (setting.type == type) {
                return setting;
            }
        }
        //this should never happened!!
        return new SpecieSetting();
    }

    public SpecieSetting GetCurrentSpecieSetting() {
        return GetSpecieSetting(specieType);
    }

    private void Build()
    {

        HighligthColor = _highligthColor;
        SecondaryColor = _secondaryColor;
        SurfaceColor = _surfaceColor;
        OnSurfaceColor = _onSurfaceColor;
        BackgroundColor = _backgroundColor;
        IconColor = _iconColor;
        DisabledIconColor = _disabledIconColor;
        TaskBarColor = _taskBarColor;

        SetSecondaryColor[] secondaryElements = FindObjectsOfType<SetSecondaryColor>();
        SetSurfaceColor[] surfaceElements = FindObjectsOfType<SetSurfaceColor>();
        SetTaskbarColor taskbar = FindObjectOfType<SetTaskbarColor>();

        foreach (var item in secondaryElements)
            item.GetComponent<Image>().color = SecondaryColor;

        foreach (var item in surfaceElements)
            item.GetComponent<Image>().color = SurfaceColor;

        if (Camera.main != null)
            Camera.main.backgroundColor = BackgroundColor;
        if (taskbar != null)
            taskbar.GetComponent<Image>().color = TaskBarColor;
    }

    IEnumerator SanityCheck() {
        yield return new WaitForEndOfFrame();
        Transform[] all = globalParent.GetComponentsInChildren<Transform>(true);
        print("checking " + all.Length + " objects");
        int noNameCount = 0;
        foreach (Transform t in all) {
            if(string.IsNullOrEmpty(t.name) || string.IsNullOrWhiteSpace(t.name)) {
                noNameCount++;
                MeshFilter meshFilter = t.GetComponent<MeshFilter>();
                if (meshFilter != null) {
                    print(meshFilter.sharedMesh.name + " has empty name");
                }
            }
        }
        print("found " + noNameCount + " objects with empty name");
    }

    IEnumerator UrlNavidSelection() {

        if (!string.IsNullOrEmpty(UrlParser.openNavid)) {

            int cullingMask = Camera.main.cullingMask;
            Camera.main.cullingMask = 0;

            yield return new WaitForEndOfFrame();
            //wait one more frame to let the label's line initialized properly!
            yield return new WaitForEndOfFrame();
        

            print("Trying to focus on " + UrlParser.openNavid + " navid object");

            TextAsset navidFile = Resources.Load<TextAsset>(globalParent.name.Replace("@", "").ToLower() + "_navid");
            if (navidFile != null) {
                print("find navid file: " + navidFile.name);
                Dictionary<string, string>  navidsMap = new Dictionary<string, string>();
                string[] lines = navidFile.text.Split("\n", System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    string[] tokens = line.Split(";", System.StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length != 2) {
                        print("Issue in navid line: " + line);
                    }
                    else {
                        if (!navidsMap.ContainsKey(tokens[1])) {
                            //reversed map: name, navid
                            navidsMap.Add(tokens[1], tokens[0]);
                        }
                        else {
                            print("navid map creation :: " + tokens[1] + " already exists!");
                        }
                    }
                }
                print("navid file parsed successfully!");

                foreach(NameAndDescription nameScript in allNameScripts) {
                    string name = nameScript.name.Replace("(R)", "").Replace("(L)", "").Trim().RemoveSuffix();
                    if (navidsMap.ContainsKey(name)) {
                        if (navidsMap[name] == UrlParser.openNavid) {
                            TangibleBodyPart part = nameScript.GetComponent<TangibleBodyPart>();
                            Label label = nameScript.GetComponent<Label>();
                            if(part != null) {
                                print("focusing on " + part.name + " (name=" + name + ")");
                                part.ObjectClicked();
                                FindObjectOfType<ContextualMenu>(true).IsolateClick();
                                CameraController.instance.CenterView(true);
                                Camera.main.cullingMask = cullingMask;
                                yield break;
                            }
                            else if(label != null) {
                                print("focusing on " + label.name + " (name=" + name + ")");
                                label.Click();
                                FindObjectOfType<ContextualMenu>(true).IsolateClick();
                                CameraController.instance.CenterView(true);
                                Camera.main.cullingMask = cullingMask;
                                yield break;
                            }
                            else {
                                print("searched object '" + name + "' has no tangibleBodyPart or label script");
                            }
                        }
                    }
                    else {
                        print("cannot find '" + name + "' in navids map");
                    }
                }
            }
            else {
                print("navid file cannot be found for " + globalParent.name);
            }

            print("unable to focus on " + UrlParser.openNavid);
            Camera.main.cullingMask = cullingMask;
        }
    }

}
