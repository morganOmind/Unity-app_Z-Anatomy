using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class StaticMethods
{

    public static Transform RecursiveFindChild(this Transform parent, string childName)
    {
        childName = childName.Replace(".t", "").Replace(".s", "").ToLower();
        foreach (Transform child in parent)
        {
            string childN = child.name.Replace(".t", "").Replace(".s","").ToLower();
            
            if (childN == childName && !child.CompareTag("Insertions") && !childN.Contains(".j") && !childN.Contains(".i"))
            {
                return child;
            }
            else
            {
                Transform found = child.RecursiveFindChild(childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        
        return null;
    }

    public static string RemovePunctuations(this string input)
    {
        return Regex.Replace(input, "[\"(),./:;\\[\\]{}]", string.Empty);
    }
    public static int ParentCount(this Transform parent)
    {
        int count = 0;
        while(parent.parent != null)
        {
            parent = parent.parent;
            count++;
        }
        return count;
    }

    public static string RemoveAccents(this string text) =>
        new String(
            text.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray()
        )
        .Normalize(NormalizationForm.FormC);

    public static void SetActiveRecursively(this Transform parent, bool state, List<GameObject> changed = null)
    {
        if (!parent.name.Contains(".j") && !parent.name.Contains(".i") && parent.GetComponent<Label>() == false && !parent.CompareTag("Insertions"))
        {
            if (changed != null && ((state && !parent.gameObject.activeInHierarchy) || (!state && parent.gameObject.activeInHierarchy)))
                changed.Add(parent.gameObject);
            parent.gameObject.SetActive(state);
            BodyPartVisibility parentVisibility = parent.GetComponent<BodyPartVisibility>();
            if (parentVisibility != null)
                parentVisibility.isVisible = state;
        }
        foreach (Transform child in parent)
        {
            if(!child.name.Contains(".j") && !child.name.Contains(".i") && child.GetComponent<Label>() == false && !parent.CompareTag("Insertions"))
            {
                if (changed != null && ((state && !child.gameObject.activeInHierarchy) || (!state && child.gameObject.activeInHierarchy)))
                    changed.Add(child.gameObject);
                child.gameObject.SetActive(state);
                BodyPartVisibility childVisibility = child.GetComponent<BodyPartVisibility>();
                if (childVisibility != null)
                    childVisibility.isVisible = state;
            }
            if (child.childCount > 0)
                child.SetActiveRecursively(state);
        }
    }

    public static void SetActiveParentsRecursively(this Transform parent, bool state, List<GameObject> changed = null)
    {
        if (parent.CompareTag("GlobalParent") || parent == null || parent.name.Contains(".j") || parent.name.Contains(".i"))
            return;
        if (changed != null && ((state && !parent.gameObject.activeInHierarchy) || (!state && parent.gameObject.activeInHierarchy)))
            changed.Add(parent.gameObject);
        parent.gameObject.SetActive(state);
        BodyPartVisibility isVisible = parent.GetComponent<BodyPartVisibility>();
        if (isVisible != null)
            isVisible.isVisible = true;
        parent.parent.SetActiveParentsRecursively(state, changed);
    }

    public static Texture2D ResampleAndCrop(Texture2D source, int targetWidth, int targetHeight)
    {
        int sourceWidth = source.width;
        int sourceHeight = source.height;
        float sourceAspect = (float)sourceWidth / sourceHeight;
        float targetAspect = (float)targetWidth / targetHeight;
        int xOffset = 0;
        int yOffset = 0;
        float factor = 1;
        if (sourceAspect > targetAspect)
        { // crop width
            factor = (float)targetHeight / sourceHeight;
            xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
        }
        else
        { // crop height
            factor = (float)targetWidth / sourceWidth;
            yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
        }
        Color32[] data = source.GetPixels32();
        Color32[] data2 = new Color32[targetWidth * targetHeight];
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                var p = new Vector2(Mathf.Clamp(xOffset + x / factor, 0, sourceWidth - 1), Mathf.Clamp(yOffset + y / factor, 0, sourceHeight - 1));
                // bilinear filtering
                var c11 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                var c12 = data[Mathf.FloorToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                var c21 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.FloorToInt(p.y))];
                var c22 = data[Mathf.CeilToInt(p.x) + sourceWidth * (Mathf.CeilToInt(p.y))];
                var f = new Vector2(Mathf.Repeat(p.x, 1f), Mathf.Repeat(p.y, 1f));
                data2[x + y * targetWidth] = Color.Lerp(Color.Lerp(c11, c12, p.y), Color.Lerp(c21, c22, p.y), p.x);
            }
        }

        var tex = new Texture2D(targetWidth, targetHeight);
        tex.SetPixels32(data2);
        tex.Apply(true);
        return tex;
    }


    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }


    public static IEnumerable<int> AllIndexesOf(this string sourceString, string subString)
    {
        subString = Regex.Escape(subString);
        foreach (Match match in Regex.Matches(sourceString, subString))
        {
            yield return match.Index;
        }
    }

    public static string RemoveSuffix(this string str)
    {
        int indexOfPoint = str.LastIndexOf('.');
        if (indexOfPoint != -1)
        {
            string suffix = str.Substring(indexOfPoint);
            if(suffix.Length <= 5)
                str = str.Replace(suffix, "");
        }
        return str;
    }

    public static bool IsRight(this string str)
    {
        int indexOfPoint = str.LastIndexOf('.');
        if (indexOfPoint != -1)
        {
            string suffix = str.Substring(indexOfPoint);
            return suffix.ToLower().Contains("r");
        }
        return false;
    }

    public static bool IsLeft(this string str)
    {
        int indexOfPoint = str.LastIndexOf('.');
        if (indexOfPoint != -1)
        {
            string suffix = str.Substring(indexOfPoint);
            return suffix.ToLower().Contains("l");
        }
        return false;
    }

    public static string RemoveRichTextTags(this string text)
    {
        int index = text.IndexOf('<');
        int index2 = text.IndexOf('>');
        while(index != -1)
        {
            text = text.Remove(index, index2 - index + 1);
            index = text.IndexOf('<');
            index2 = text.IndexOf('>');
        }

        return text;
    }

    public static string RemoveBodyPartLinks(this string text)
    {
        return text.Replace("<link><color=#EA7600>", "").Replace("</link></color>", "");
    }

    public static bool IsBodyPart(this GameObject go)
    {
        return go != null && go.GetComponent<TangibleBodyPart>() != null;
    }

    public static bool IsLabel(this GameObject go)
    {
        return go != null && go.GetComponent<Label>() != null;
    }

    public static bool IsGroup(this GameObject go)
    {
        return go != null && go.GetComponent<NameAndDescription>() != null && go.GetComponent<NameAndDescription>().originalName.EndsWith(".g");
    }

    public static Bounds GetBounds(List<GameObject> objects)
    {
        //List of selected scripts
        List<TangibleBodyPart> scripts = new List<TangibleBodyPart>();

        foreach (var obj in objects)
        {
            var script = obj.GetComponent<TangibleBodyPart>();
            if (script != null)
                scripts.Add(script);
        }

        if (scripts.Count == 0)
            return new Bounds();

        TangibleBodyPart first = scripts[0];

        //Calculate bounds for all objects
        Bounds bounds = new Bounds(first.center, first.bounds.size);

        for (int i = 1; i < scripts.Count; i++)
        {
            if (scripts[i] != null && scripts[i].bounds != null)
                bounds.Encapsulate(scripts[i].bounds);
        }

        return bounds;
    }

    public static T[] GetComponentsInDirectChildren<T>(this Transform gameObject) where T : Component
    {
        List<T> components = new List<T>();
        for (int i = 0; i < gameObject.childCount; ++i)
        {
            T component = gameObject.GetChild(i).GetComponent<T>();
            if (component != null)
                components.Add(component);
        }

        return components.ToArray();
    }

    /// <summary>
    /// Is the pointer hovering over a game object with the given name
    /// </summary>
    /// <param name="name">Name of the game object hovering over</param>
    /// <returns>Status</returns>
    public static bool IsPointerOverGameObjectName(string name) {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Mouse.current.position.value;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.name == name) {
                    return true;
                }
            }
        }

        return false;
    }

    public static void CopyToClipboard(string content) {
        content = content.RemoveRichTextTags();
#if UNITY_WEBGL
        WebGLCopyAndPaste.WebGLCopyAndPasteAPI.CopyToClipboard(content);
#else
        GUIUtility.systemCopyBuffer = content;
#endif
        PopUpManagement.Instance.Show("Text copied!");
    }

    //https://www.csharpstar.com/csharp-string-distance-algorithm/
    //The Levenshtein distance is a string metric for measuring the difference between two sequences. 
    //The Levenshtein distance between two words is the minimum number of single-character edits (i.e. insertions, deletions or substitutions) 
    //required to change one word into the other. It is named after Vladimir Levenshtein.
    public static int LevenshteinDistance(string s, string t) {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0) {
            return m;
        }

        if (m == 0) {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++) {
        }

        for (int j = 0; j <= m; d[0, j] = j++) {
        }

        // Step 3
        for (int i = 1; i <= n; i++) {
            //Step 4
            for (int j = 1; j <= m; j++) {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    public static string GetDefaultSavePath() {
#if !UNITY_EDITOR && UNITY_WEBGL
        return "";
#else
        return System.IO.Directory.GetParent(Application.dataPath).FullName;
#endif
    }
}
