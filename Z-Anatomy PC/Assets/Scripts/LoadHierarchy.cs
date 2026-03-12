using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadHierarchy : MonoBehaviour
{
    public char Separator = ';';
    [Space]
    [Header("Load from this file")]
    public TextAsset file;
    private Dictionary<string, Transform> allObjects;


    private void Start()
    {
        Fetch();
        Load();
    }

    private void Fetch()
    {
        allObjects = new Dictionary<string, Transform>();
        foreach (Transform item in GlobalVariables.Instance.globalParent.GetComponentsInChildren<Transform>(true)) {
            //if error "an item with same key: minPoint", untag all minPoint and maxPoints
            if (!item.CompareTag("Untagged")) {
                NameAndDescription nameDesc = item.GetComponent<NameAndDescription>();
                //.i and .j do not have NameAndDescription component, and already keep their original name (they are never renamed)
                allObjects.Add(nameDesc == null ? item.name : nameDesc.originalName, item.transform);
            }
        }

    }

    private void Load()
    {
        Dictionary<Transform, int> ordered = new Dictionary<Transform, int>();

        string[] lines = file.text.Replace("\t", "").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            if (!line.Contains(";") || line.Length == 0)
                continue;
            int index = int.Parse(line.Split(Separator)[1]);
            if (!allObjects.ContainsKey(line.Split(Separator)[0]))
            {
                //Debug.Log(line.Split(Separator)[0]);
                continue;
            }
            ordered.Add(allObjects[line.Split(Separator)[0]], index);
        }

        foreach (var item in ordered.OrderBy(it => it.Value))
        {
            item.Key.SetSiblingIndex(item.Value);
        }
    }
}
