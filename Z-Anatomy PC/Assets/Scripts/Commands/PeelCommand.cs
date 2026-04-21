using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PeelCommand : ICommand {

    GameObject peeledObject;
    List<GameObject> shownChildren;

    public PeelCommand(GameObject obj) {
        this.peeledObject = obj;
        this.shownChildren = new List<GameObject>();
        for(int i=0; i<obj.transform.childCount; i++) {
            Transform t = obj.transform.GetChild(i);
            if(t.tag != "Insertions" && t.GetComponent<TangibleBodyPart>() != null) {
                shownChildren.Add(t.gameObject);
            }
        }
    }

    public bool IsEmpty() {
        return peeledObject == null || shownChildren == null || shownChildren.Count == 0;
    }

    public bool Equals(ICommand command) {
        return command.GetType() == GetType() 
            && (command as PeelCommand).peeledObject.Equals(peeledObject)
            && (command as PeelCommand).shownChildren.SequenceEqual(shownChildren);
    }

    public void Execute() {
        SelectedObjectsManagement.Instance.DeselectAllObjects();
        SelectedObjectsManagement.Instance.peeledObject = peeledObject;
        peeledObject.GetComponent<MeshRenderer>().enabled = false;
        peeledObject.GetComponent<MeshCollider>().enabled = false;
        peeledObject.GetComponent<BodyPartVisibility>().isVisible = false;

        foreach (GameObject item in shownChildren) {
            //item.transform.SetParent(peeledObject.transform.parent);
            SelectedObjectsManagement.Instance.SelectObject(item);
            item.GetComponent<BodyPartVisibility>().isVisible = true;
            //item.transform.SetActiveParentsRecursively(true);
        }

        if (shownChildren.Count > 0 && ActionControl.zoomSelected)
            CameraController.instance.CenterView(true);
        if (shownChildren.Count == 1)
            SelectedObjectsManagement.Instance.ShowBodyPartInfo(shownChildren[0]);

        ActionControl.Instance.UpdateButtons();
    }

    public void Undo() {
        /*foreach (GameObject item in shownChildren) {
            item.transform.SetParent(peeledObject.transform);
            SelectedObjectsManagement.Instance.DeselectObject(item);
        }*/
        SelectedObjectsManagement.Instance.DeselectAllObjects();
        SelectedObjectsManagement.Instance.SelectObject(peeledObject);
        peeledObject.GetComponent<MeshRenderer>().enabled = true;
        peeledObject.GetComponent<MeshCollider>().enabled = true;
        peeledObject.GetComponent<BodyPartVisibility>().isVisible = true;
        peeledObject.transform.SetActiveParentsRecursively(true);

        SelectedObjectsManagement.Instance.peeledObject = null;
    }
}
