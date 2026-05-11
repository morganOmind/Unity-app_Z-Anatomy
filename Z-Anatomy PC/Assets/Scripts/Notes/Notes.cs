using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class Notes : MonoBehaviour
{
    public static Notes instance;

    private RaycastHit hit;
    int layer_mask1;
    int layer_mask2;
    int layer_mask3;
    LayerMask finalmask;

    public Canvas canvas;
    public NoteGizmo gizmoPrefab;
    public GameObject notePrefab;

    [HideInInspector]
    public bool gizmoPlaced = false;
    private NoteGizmo currentGizmo;

    private Line3D currentLine;
    [SerializeField] private Line3D linePrefab;

    TangibleBodyPart clickedBp;

    public Texture2D cursorTexture;
    bool hasNoteCursor = false;

    Note lastNote;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        layer_mask1 = LayerMask.GetMask("Body");
        layer_mask2 = LayerMask.GetMask("Outline");
        layer_mask3 = LayerMask.GetMask("HighlightedOutline");
        finalmask = layer_mask1 | layer_mask2 | layer_mask3;
        currentGizmo = Instantiate(gizmoPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (!ActionControl.creatingLocalNote && !ActionControl.creatingGlobalNote) {
            if (hasNoteCursor) {
                if (cursorTexture != null)
                    Cursor.SetCursor(null, new Vector2(), CursorMode.Auto);
                hasNoteCursor = false;
            }
            if(currentLine != null 
                && (lastNote == null || lastNote != null && lastNote.line != currentLine)) {
                GameObject.Destroy(currentLine.gameObject);
            }
            currentGizmo.gameObject.SetActive(false);
            currentGizmo.placed = false;
            gizmoPlaced = false;
            
            return;
        }

        if (!hasNoteCursor) {
            if (cursorTexture != null)
                Cursor.SetCursor(cursorTexture, new Vector2(), CursorMode.Auto);
            hasNoteCursor = true;
        }

        if(!gizmoPlaced)
        {
            Ray raycast = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(raycast, out hit, 100, finalmask))
            {
                clickedBp = hit.transform.GetComponent<TangibleBodyPart>();
                if(clickedBp != null && (ActionControl.creatingGlobalNote || clickedBp == ContextualMenu.Instance.contextObject))
                {
                    currentGizmo.hit = hit;
                    currentGizmo.gameObject.SetActive(true);
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        gizmoPlaced = true;
                        currentLine = CreateLine(hit.point);
                    }
                }
                else
                    currentGizmo.gameObject.SetActive(false);
            }
            else if(!gizmoPlaced)
                currentGizmo.gameObject.SetActive(false);
        }
        else if(gizmoPlaced)
        {
            currentLine.lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                currentGizmo.placed = true;
                lastNote = CreateNote();
                currentGizmo.note = lastNote;
                
                currentGizmo = Instantiate(currentGizmo);
                currentGizmo.placed = false;
                currentGizmo.gameObject.SetActive(false);

                gizmoPlaced = false;

                StartCoroutine(WaitForRaycast());
                IEnumerator WaitForRaycast()
                {
                    yield return null;
                    ActionControl.creatingLocalNote = false;
                    ActionControl.creatingGlobalNote = false;
                    CameraController.instance.raycaster.enabled = true;

                    if (cursorTexture != null)
                        Cursor.SetCursor(null, new Vector2(), CursorMode.Auto);
                    hasNoteCursor = false;
                }
            }
        }

    }


    private Note CreateNote()
    {
        return CreateNote(currentLine, currentGizmo, Mouse.current.position.ReadValue(), clickedBp);
    }

    public Note CreateNote(Line3D line, NoteGizmo gizmo, Vector3 notePosition, TangibleBodyPart part) {
        GameObject noteGo = Instantiate(notePrefab, canvas.transform);
        Note note = noteGo.GetComponent<Note>();
        note.line = line;
        note.gizmo = gizmo;
        noteGo.transform.position = notePosition;
        if(part != null) {
            part.AddNote(note);
        }
        return note;
    }

    public Line3D CreateLine(Vector3 hitPoint) {
        Line3D line = Instantiate(linePrefab, hitPoint, Quaternion.identity);
        line.lineRenderer.positionCount = 2;
        line.lineRenderer.SetPosition(0, hitPoint);
        return line;
    }
}
