using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a label in 3D space. 
/// Updates its color based on its position and handles its selection.
/// </summary>
public class Label : MonoBehaviour
{
    private Camera cam;
    GameObject textGO;
    private TextMeshPro text;
    private float parentScale;
    private MaterialPropertyBlock _propBlock;
    private Renderer _renderer;
    private Transform originPoint;
    private BoxCollider boxCollider;
    RectTransform rect;
    [HideInInspector]
    public Material labelMaterial;
    [HideInInspector]
    public Line line;
    private NameAndDescription nameScript;
    private BodyPartVisibility visibilityScript;
    private bool hasLine;
    private Color color;
    private float fontSize;

    [HideInInspector]
    public TangibleBodyPart parent;

    [HideInInspector]
    public Vector3 lineDirection;

    private void OnEnable() {
        if(textGO != null) {
            textGO.SetActive(true);
        }
    }

    private void OnDisable() {
        if(textGO != null) {
            textGO.SetActive(false);
        }
    }

    private void Awake()
    {
        TextMeshPro tmp = GetComponent<TextMeshPro>();
        if(tmp != null) {
            Component.Destroy(tmp);
        }
        Renderer rend = GetComponent<Renderer>();
        if(rend != null) {
            Component.Destroy(rend);
        }
        MeshFilter mesh = GetComponent<MeshFilter>();
        if(mesh != null) {
            Component.Destroy(mesh);
        }

        nameScript = GetComponent<NameAndDescription>();
        visibilityScript = GetComponent<BodyPartVisibility>();
        

        textGO = new GameObject(gameObject.name + "-TMP");
        textGO.transform.parent = FindClosestParentNoLabelNoLine();
        textGO.transform.position = transform.position;
        textGO.layer = LayerMask.NameToLayer("Body");

        LabelText lt = textGO.AddComponent<LabelText>();
        lt.label = this;

        text = textGO.AddComponent<TextMeshPro>();
        boxCollider = textGO.AddComponent<BoxCollider>();
        rect = textGO.GetComponent<RectTransform>();
        cam = Camera.main;
        color = Color.white;
        parent = GetComponentInParent<TangibleBodyPart>();

        Transform maxPoint = null;
        Transform minPoint = null;

        try {
            var lineObj = transform.parent.Find(new StringBuilder().Append(nameScript.originalName.Replace(".t", "").Replace(".s", "")).Append(".j").ToString());
            if (lineObj != null)
                line = lineObj.GetComponent<Line>();
            else
                line = transform.parent.Find(new StringBuilder().Append(nameScript.originalName.Replace(".t", "").Replace(".s", "")).Append(".i").ToString()).GetComponent<Line>();

            // be sure to activate the line because is is disable for the legacy human model at import setup
            if (GlobalVariables.Instance.GetCurrentSpecieSetting().type == SpecieType.Man) {
                line.gameObject.SetActive(true);
            }

            //find true origin point (because it is swapped between legacy human import models and new ones !)
            maxPoint = line.transform.Find("maxPoint");
            minPoint = line.transform.Find("minPoint");
            float maxDist = Vector3.Distance(maxPoint.position, transform.position);
            float minDist = Vector3.Distance(minPoint.position, transform.position);

            originPoint = maxDist > minDist ? maxPoint : minPoint;

            hasLine = true;
        }
        catch (System.Exception) {
            hasLine = false;
        }

        if (line != null && minPoint != null && maxPoint != null)
            lineDirection = maxPoint.position - minPoint.position;
        else
            lineDirection = parent.transform.position - transform.position;

        Initialize();
    }

    Transform FindClosestParentNoLabelNoLine() {
        Transform t = transform.parent;
        while(t.GetComponent<Line>() != null || t.GetComponent<Label>() != null) {
            t = t.parent;
        }
        return t;
    }

    int sceneLoadedFrame;
    private void OnLevelWasLoaded(int level) {
        sceneLoadedFrame = Time.frameCount;
    }

    private void Start() {
        //only disable on startup, else some sublabels are disabled before their start, and disable themselves at first enable after startup and do not show up the first time!
        //this way, we are sure that every labels is disable at startup, while not disabling itself when we want to show it for the first time
        if (Time.frameCount - sceneLoadedFrame <= 1) {
            foreach (Label l in GetComponentsInChildren<Label>(true)) {
                l.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);

        }
    }

    /// <summary>
    /// Initializes the Label component, setting its properties and text value.
    /// </summary>
    public void Initialize()
    {
        // Set the box collider center to the center of the text mesh.
        boxCollider.center = Vector3.zero;
        // Store the parent scale value for later use.
        parentScale = transform.parent.lossyScale.x;

        // Set text properties, such as margins, alignment, font size, material, and so on.
        text.margin = new Vector4(8f, 2f, 8f, 2f);
        text.alignment = TextAlignmentOptions.Center;
        fontSize = GlobalVariables.Instance.labelFontSize * 0.15f;
        text.fontSize = fontSize * Mathf.Clamp(cam.orthographicSize, 0.075f, 1.5f);
        text.material = labelMaterial;

        // Set the scale of the rect transform to account for the parent scale.
        rect.localScale = new Vector3(1 / parentScale, 1 / parentScale, 1 / parentScale);

        // Create a new material property block for the label.
        _propBlock = new MaterialPropertyBlock();
        _renderer = text.renderer;

        // Set the label tex
        SetText(gameObject.name.Replace(".j", "").Replace(".i", "").Replace(".t", "").Replace(".s",""));
    }

    // Update is called once per frame
    void Update()
    {
        boxCollider.size = text.textBounds.size;
        textGO.transform.rotation = cam.transform.rotation;
        text.fontSize = fontSize * Mathf.Clamp(cam.orthographicSize, 0.075f, 1.5f);

        if (hasLine)
            UpdateColor();
    }

    /// <summary>
    /// Updates the color of the object based on the angle between the origin point and camera direction.
    /// </summary>
    /// <remarks>
    /// This function calculates the angle between the origin point and camera direction, and then uses the angle to calculate the new color of the object.
    /// The color is then set to the object and also to the line renderer attached to the object. 
    /// If the object is selected, the line color is set to the new color.
    /// </remarks>
    /// <returns>
    void UpdateColor()
    {
        float angle = Vector3.Angle(originPoint.position - transform.position, -cam.transform.forward);
        float a = angle * angle * angle * angle * 0.000000025f;
        if(_renderer != null) {
            _renderer.enabled = a > .075f;
            if (line._renderer != null)
                line._renderer.enabled = _renderer.enabled;

            if (a > 1)
                a = 1;

            Color newColor = new Color(color.r, color.g, color.b, a);

            SetColor(newColor);
        }
    }

    public void SetColor(Color newColor) {
        // Get the current value of the material properties in the renderer.
        _renderer.GetPropertyBlock(_propBlock);
        // Assign our new value.
        _propBlock.SetColor("_FaceColor", newColor);
        // Apply the edited values to the renderer.
        _renderer.SetPropertyBlock(_propBlock);

        if (visibilityScript.isSelected)
            line.SetColor(newColor);
        else
            line.SetColor(newColor.a * 0.5f);
    }

    public Color GetCurrentColor() {
        _renderer.GetPropertyBlock(_propBlock);
        return _propBlock.GetColor("_FaceColor");
    }

    /// <summary>
    /// Sets the text of the label.
    /// </summary>
    /// <remarks>
    /// If the 'name' parameter matches the substring obtained from the parent object's name, the font style is set to 'Bold' and the font size is set to a scaled value.
    /// </remarks>
    public void SetText(string name)
    {
        if(text != null)
        {
            text.text = name;
            string substring = transform.parent.name.Replace("(R)", "").Replace("(L)", "").Trim();
            if (substring.Contains("."))
            {
                int indexOfPoint = transform.parent.name.IndexOf('.');
                substring = transform.parent.name.Substring(0, indexOfPoint);
            }
            if (name.Equals(substring))
            {
                text.fontStyle = FontStyles.Bold;
                fontSize = GlobalVariables.Instance.titleLabelFontSize * 0.15f;
            }
        }
    }


    /// <summary>
    /// Handles the click event of the label and updates the UI.
    /// </summary>
    /// <remarks>
    /// This function checks if the game object is not already selected. 
    /// If not, it deselects all other children of the parent object and selects the game object.
    /// Then updates the hierarchy bar, expands the lexicon and sets the description. 
    /// If the game object is already selected, it deselects the object. 
    /// </remarks>
    public void Click()
    {
        if (!SelectedObjectsManagement.Instance.selectedObjects.Contains(gameObject))
        {
            SelectedObjectsManagement.Instance.DeselectAllChildren(gameObject.transform.parent);
            SelectedObjectsManagement.Instance.SelectObject(gameObject);

            //Update hierarchy bar
            HierarchyBar.Instance.Set(transform);
            //Expand in lexicon
            Lexicon.Instance.ExpandRecursively();
            nameScript.SetDescription();
        }
        else
        {
            SelectedObjectsManagement.Instance.DeselectObject(gameObject);
            GetComponent<BodyPartVisibility>().isVisible = false;
        }
        ActionControl.Instance.UpdateButtons();
    }

    /// <summary>
    /// Sets the color of the label and line to the highlight color.
    /// </summary>
    public void Select()
    {
        color = GlobalVariables.HighligthColor;
        if(!hasLine && _renderer != null)
        {
            // Get the current value of the material properties in the renderer.
            _renderer.GetPropertyBlock(_propBlock);
            // Assign our new value.
            _propBlock.SetColor("_FaceColor", color);
            // Apply the edited values to the renderer.
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    /// <summary>
    /// Sets the color of the label and line to the default color.
    /// </summary>
    public void Deselect()
    {
        color = Color.white;
        if (!hasLine && _renderer != null)
        {
            // Get the current value of the material properties in the renderer.
            _renderer.GetPropertyBlock(_propBlock);
            // Assign our new value.
            _propBlock.SetColor("_FaceColor", color);
            // Apply the edited values to the renderer.
            _renderer.SetPropertyBlock(_propBlock);
        }
    }

    public void UpdatePos()
    {
        if(line != null)
            line.UpdatePos();
    }
}

