 using UnityEngine;
using UnityEditor;


namespace WorldSpaceTransitions
{
    [CustomEditor(typeof(SectionSetup))]
    public class SectionSetupEditor : Editor
    {
        string shaderInfo = "";
        SerializedProperty m_model;
        void OnEnable()
        {
            m_model = serializedObject.FindProperty("model");
        }
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector(); 
            SectionSetup setupScript = (SectionSetup)target;
            serializedObject.Update();
            //setupScript.model = (GameObject)EditorGUILayout.ObjectField("model", setupScript.model, typeof(GameObject), true);
            EditorGUILayout.PropertyField(m_model);
            serializedObject.ApplyModifiedProperties();
            if (setupScript.model)
            {
                if (setupScript.GetComponent<ISizedSection>() != null)
                {
                    setupScript.boundsMode = (BoundsOrientation)EditorGUILayout.EnumPopup("bounds mode:", setupScript.boundsMode);
                    if(GUI.changed) setupScript.RecalculateBounds();
                    //setupScript.accurateBounds = EditorGUILayout.Toggle("accurate bounds", setupScript.accurateBounds);
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Recalculate bounds of " + setupScript.model.name))
                    {
                        setupScript.RecalculateBounds();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                //GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check shaders on " + setupScript.model.name))
                {
                    shaderInfo = setupScript.CheckShaders();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (shaderInfo != "") GUILayout.Label(shaderInfo);
            }
            if (setupScript.shaderSubstitutes.Count > 0)
            {
                DrawDefaultInspector(); //draw shaderSubstitutes
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create and Assign Section Materials"))
                {
                    setupScript.CreateSectionMaterials();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }
}

