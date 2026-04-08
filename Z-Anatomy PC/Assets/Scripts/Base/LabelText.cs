using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelText : MonoBehaviour
{
    public Label label;

    //prevent enabling from various script as undesired behavior!
    private void OnEnable() {
        if(label != null) {
            if (!label.gameObject.activeSelf) {
                gameObject.SetActive(false);
            }
        }
    }
}
