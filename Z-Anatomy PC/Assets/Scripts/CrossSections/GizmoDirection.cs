using UnityEngine;

public class GizmoDirection : MonoBehaviour
{
    [SerializeField] private GizmoFace gizmoFace;

    public GizmoFace GizmoFace { get => gizmoFace; set => gizmoFace = value; }
}
