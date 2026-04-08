using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialsDB : MonoBehaviour
{
    public List<Material> allMaterials;

    public static MaterialsDB instance;

    private void Awake() {
        instance = this;
    }
}
