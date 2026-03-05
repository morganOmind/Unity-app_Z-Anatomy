using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSectionPlane : MonoBehaviour
{
    /*public MeshManagement meshManagement;
    public string planePos;
    public string planeNormal;*/

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
        /*for (int i = 0; i < GlobalVariables.Instance.allBodyPartRenderers.Count; i++)
        {
            foreach (Material material in GlobalVariables.Instance.allBodyPartRenderers[i].materials)
            {
                //material.SetVector(planePos, transform.position);
                //material.SetVector(planeNormal, transform.up);
            }
        }*/
        Shader.SetGlobalVector("_SectionPoint", transform.position);
        Shader.SetGlobalVector("_SectionPlane", -transform.up);
        Shader.SetGlobalVector("_SectionPlane2", transform.right);
    }

    /*private void OnDisable()
    {
        for (int i = 0; i < GlobalVariables.Instance.allBodyPartRenderers.Count; i++)
        {
            foreach (Material material in GlobalVariables.Instance.allBodyPartRenderers[i].materials)
            {
                material.SetVector(planePos, Vector3.zero);
                material.SetVector(planeNormal, Vector3.zero);
            }
        }
    }*/

    /*private void OnEnable()
    {
        SendPositionToShader();
    }*/

    void Start() {
        Shader.DisableKeyword("CLIP_NONE");
        Shader.EnableKeyword("CLIP_PLANE");
    }


    void OnEnable() {
        SendPositionToShader();

        Shader.DisableKeyword("CLIP_NONE");
        Shader.EnableKeyword("CLIP_PLANE");
    }

    void OnDisable() {

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
