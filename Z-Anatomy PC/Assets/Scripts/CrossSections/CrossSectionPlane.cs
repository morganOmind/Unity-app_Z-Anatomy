using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSectionPlane : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            SendPositionToShader();
        }
    }

    public void SendPositionToShader()
    {
        for (int i = 0; i < GlobalVariables.Instance.allBodyPartRenderers.Count; i++)
        {
            foreach (Material material in GlobalVariables.Instance.allBodyPartRenderers[i].materials)
            {
                material.SetVector("_PlanePosition", transform.position);
                material.SetVector("_PlaneNormal", transform.up);
            }
        }
        Shader.SetGlobalVector("_SectionPoint", transform.position);
        Shader.SetGlobalVector("_SectionPlane", -transform.up);
        Shader.SetGlobalVector("_SectionPlane2", transform.right);
    }


    void Start() {
        Shader.DisableKeyword("CLIP_NONE");
        Shader.EnableKeyword("CLIP_PLANE");
    }


    void OnEnable() {

        float lineWidth = GlobalVariables.Instance.GetCurrentSpecieSetting().crossSectionLineWidth;
        LineRenderer line = GetComponentInChildren<LineRenderer>();
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        SendPositionToShader();
        //some materials did not initialized properly, so make this fix to prevent that
        //(and keep the original direct call before to prevent a weird clipping visual artefact)
        StartCoroutine(LateInit());

        Shader.DisableKeyword("CLIP_NONE");
        Shader.EnableKeyword("CLIP_PLANE");
    }

    IEnumerator LateInit() {
        yield return new WaitForEndOfFrame();
        SendPositionToShader();
    }

    void OnDisable() {

        for (int i = 0; i < GlobalVariables.Instance.allBodyPartRenderers.Count; i++) {
            foreach (Material material in GlobalVariables.Instance.allBodyPartRenderers[i].materials) {
                material.SetVector("_PlanePosition", Vector3.zero);
                material.SetVector("_PlaneNormal", Vector3.zero);
            }
        }

        Shader.SetGlobalVector("_SectionPoint", Vector3.zero);
        Shader.SetGlobalVector("_SectionPlane", Vector3.zero);
        Shader.SetGlobalVector("_SectionPlane2", Vector3.zero);

        Shader.DisableKeyword("CLIP_PLANE");
        Shader.EnableKeyword("CLIP_NONE");
    }

    void OnApplicationQuit() {
        //disable clipping so we could see the materials and objects in editor properly
        Shader.DisableKeyword("CLIP_PLANE");
        Shader.EnableKeyword("CLIP_NONE");

    }

}
