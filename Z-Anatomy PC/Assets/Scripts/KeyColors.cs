using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class KeyColors : MonoBehaviour
{

    public static KeyColors Instance;

    public float skeletonWeight;
    public float insertionsWeight;
    public float musclesWeight;
    public float nervousWeight;
    public float lymphsWeight;
    public float regionsWeight;
    public float visceralWeight;

    [Header("Primary colors")]
    public List<Material> skeletonPrimary;
    public List<Material> insertionsPrimary;
    public List<Material> musclesPrimary;
    public List<Material> nervousPrimary;
    public List<Material> lymphsPrimary;
    public List<Material> regionsPrimary;
    public List<Material> visceralPrimary;

    [Space]
    [Space]
    [Header("Secondary colors")]
    public List<Material> skeletonSecondary;
    public List<Material> insertionsSecondary;
    public List<Material> musclesSecondary;
    public List<Material> nervousSecondary;
    public List<Material> lymphsSecondary;
    public List<Material> regionsSecondary;
    public List<Material> visceralSecondary;

    private bool bonesEnabled;
    private bool insertionsEnabled;
    private bool muscularEnabled;
    private bool nervesEnabled;
    private bool lymphsEnabled;
    private bool skinEnabled;
    private bool visceraEnabled;

    public bool update;

    private void Awake()
    {
        Instance = this;
    }

    public int FindPrimaryIndex(List<Material> primaries, Material primary) {
        int index = -1;

        //original search (made for human)
        index = primaries.IndexOf(primary);
        if (index != -1)
            return index;

        //fallback: search by name for the cat  (notice that everything should be searched by name for a better integration!)
        index = primaries.FindIndex(delegate (Material m)
        {
            if (m != null)
                return m.name.ToLower().Replace("(instance)", "").Trim() == primary.name.ToLower().Replace("(instance)", "").Trim();
            else
                return false;
        });

        return index;
    }

    Material FindSecondaryMaterial(List<Material> primaries, List<Material> secondaries, Material primary) {
        int index = FindPrimaryIndex(primaries, primary);
        if (index != -1)
            return secondaries[index];

        return null;
    }

    public Material GetSecondaryColor(string tag, Material primary)
    {
        Material secondary = null;
        bool wasInSearch = false;
        switch (tag)
        {
            case "Skeleton":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(skeletonPrimary, skeletonSecondary, primary);
                break;

            /*case "Joints":
                return jointsToggle.isOn;*/

            case "Insertions":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(insertionsPrimary, insertionsSecondary, primary);
                break;

            case "Lymph":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(lymphsPrimary, lymphsSecondary, primary);
                break;

            case "Muscles":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(musclesPrimary, musclesSecondary, primary);
                break;

           /* case "Vascular":
                return cardiovascularToggle.isOn;*/

            case "Nervous":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(nervousPrimary, nervousSecondary, primary);
                break;

            case "Visceral":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(visceralPrimary, visceralSecondary, primary);
                break;

            case "BodyParts":
                wasInSearch = true;
                secondary = FindSecondaryMaterial(regionsPrimary, regionsSecondary, primary);
                break;

            /* case "Fascia":
                 return muscularToggle.isOn;*/

            /*case "References":
                return referencesToggle.isOn;*/

            default:
                break;
        }

        if(wasInSearch && secondary == null)
            Debug.Log("Material not found: " + tag + " :: " + primary);
        return secondary;
    }

    private void SetLayerMaterial(List<TangibleBodyPart> bodyParts, bool state, SwitchButton CrossSectionsToggle)
    {
        if(state)
        {
            foreach (var bodyPart in bodyParts)
                bodyPart.SetSecondaryMaterial(CrossSectionsToggle.isOn);
        }
        else
        {
            foreach (var bodyPart in bodyParts)
                bodyPart.SetPrimaryMaterial(CrossSectionsToggle.isOn);
        }

        if(CrossSections.Instance.activePlane != null)
            CrossSections.Instance.activePlane.SendPositionToShader();
    }

    public void SetBonesKeyColors()
    {
        bonesEnabled = !bonesEnabled;
        SetLayerMaterial(GlobalVariables.Instance.bones, bonesEnabled, CrossSections.Instance.skeletalToggle);
    }

    public void SetInsertionsKeyColor()
    {
        insertionsEnabled = !insertionsEnabled;
        SetLayerMaterial(GlobalVariables.Instance.insertions, insertionsEnabled, CrossSections.Instance.skeletalToggle);
    }

    public void SetMusclesKeyColors()
    {
        muscularEnabled = !muscularEnabled;
        SetLayerMaterial(GlobalVariables.Instance.muscles, muscularEnabled, CrossSections.Instance.muscularToggle);
        SetInsertionsKeyColor();
    }

    public void SetNervesKeyColors()
    {
        nervesEnabled = !nervesEnabled;
        SetLayerMaterial(GlobalVariables.Instance.nerves, nervesEnabled, CrossSections.Instance.nervousToggle);
    }

    public void SetLymphsKeyColors()
    {
        lymphsEnabled = !lymphsEnabled;
        SetLayerMaterial(GlobalVariables.Instance.lymphs, lymphsEnabled, CrossSections.Instance.lymphsToggle);
    }

    public void SetVisceraKeyColors()
    {
        visceraEnabled = !visceraEnabled;
        SetLayerMaterial(GlobalVariables.Instance.viscera, visceraEnabled, CrossSections.Instance.visceralToggle);
    }

    public void SetRegionsKeyColors()
    {
        skinEnabled = !skinEnabled;
        SetLayerMaterial(GlobalVariables.Instance.regions, skinEnabled, CrossSections.Instance.regionsToggle);
    }

    public void ResetAll()
    {
        bonesEnabled = false;
        insertionsEnabled = false;
        muscularEnabled = false;
        nervesEnabled = false;
        lymphsEnabled = false;
        visceraEnabled = false;
        skinEnabled = false;

        SetLayerMaterial(GlobalVariables.Instance.bones, bonesEnabled, CrossSections.Instance.skeletalToggle);
        SetLayerMaterial(GlobalVariables.Instance.insertions, insertionsEnabled, CrossSections.Instance.skeletalToggle);
        SetLayerMaterial(GlobalVariables.Instance.muscles, muscularEnabled, CrossSections.Instance.muscularToggle);
        SetLayerMaterial(GlobalVariables.Instance.nerves, nervesEnabled, CrossSections.Instance.nervousToggle);
        SetLayerMaterial(GlobalVariables.Instance.lymphs, lymphsEnabled, CrossSections.Instance.lymphsToggle);
        SetLayerMaterial(GlobalVariables.Instance.viscera, visceraEnabled, CrossSections.Instance.visceralToggle);
        SetLayerMaterial(GlobalVariables.Instance.regions, skinEnabled, CrossSections.Instance.regionsToggle);

    }

    public bool HasKeyColor(string tag)
    {
        switch (tag)
        {
            case "Skeleton":
                return true;

            case "Insertions":
                return true;

            case "Muscles":
                return true;

            case "Nervous":
                return true;

            case "Lymph":
                return true;

            case "Visceral":
                return true;

            case "BodyParts":
                return true;
        }

        return false;
    }

    public List<Material> GetPrimariesByTag(string tag) {
        List<Material> primaries = null;
        switch (tag) {
            case "Skeleton":
                primaries = skeletonPrimary;
                break;

            case "Insertions":
                primaries = insertionsPrimary;
                break;

            case "Lymph":
                primaries = lymphsPrimary;
                break;

            case "Muscles":
            case "Joints":  //dont know why, primary joints materials are in muscles list, and dont have secondary materials!!
                primaries = musclesPrimary;
                break;

            case "Nervous":
                primaries = nervousPrimary;
                break;

            case "Visceral":
                primaries = visceralPrimary;
                break;

            case "BodyParts":
                primaries = regionsPrimary;
                break;

            default:
                break;
        }
        return primaries;

    }

    public float GetWeightByTag(string tag)
    {
        switch (tag)
        {
            case "Skeleton":
                return skeletonWeight;

            case "Insertions":
                return insertionsWeight;

            case "Muscles":
                return musclesWeight;

            case "Nervous":
                return nervousWeight;

            case "Lymph":
                return lymphsWeight;

            case "Visceral":
                return visceralWeight;

            case "BodyParts":
                return regionsWeight;
        }

        return 0.35f;
    }

    public bool IsActiveByTag(string tag)
    {
        switch (tag)
        {
            case "Skeleton":
                return bonesEnabled;

            case "Insertions":
                return insertionsEnabled;

            case "Muscles":
                return muscularEnabled;

            case "Nervous":
                return nervesEnabled;

            case "Lymph":
                return lymphsEnabled;

            case "Visceral":
                return visceraEnabled;

            case "BodyParts":
                return skinEnabled;
        }

        return false;
    }

    public void EnableByTag(string tag)
    {
        switch (tag)
        {
            case "Skeleton":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetBonesKeyColors), false);
                SetBonesKeyColors();
                break;

            case "Insertions":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetInsertionsKeyColor), false);
                SetInsertionsKeyColor();
                break;

            case "Muscles":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetMusclesKeyColors), false);
                SetMusclesKeyColors();
                break;

            case "Nervous":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetNervesKeyColors), false);
                SetNervesKeyColors();
                break;

            case "Lymph":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetLymphsKeyColors), false);
                SetLymphsKeyColors();
                break;

            case "Visceral":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetVisceraKeyColors), false);
                SetVisceraKeyColors();
                break;

            case "BodyParts":
                ActionControl.Instance.AddCommand(new KeyColorCommand(SetRegionsKeyColors), false);
                SetRegionsKeyColors();
                break;
        }

    }
}
