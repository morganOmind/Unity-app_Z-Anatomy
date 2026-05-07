using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    public bool loadOnStart = false;

    public float duration;
    public int sceneIndex;
    public Image fadeBackground, loading;

    public List<GameObject> showOnLoad, hideOnLoad;
    
    private IEnumerator Start()
    {
        if(loading != null) {
            loading.fillAmount = 0f;
        }

        foreach (GameObject go in showOnLoad) {
            go.SetActive(false);
        }

        if (fadeBackground != null) {
            yield return StartCoroutine(Fade());
        }

        if (loadOnStart) {
            GoLoadScene();
        }
    }

    public void GoLoadScene() {
        foreach(GameObject go in showOnLoad) {
            go.SetActive(true);
        }
        foreach(GameObject go in hideOnLoad) {
            go.SetActive(false);
        }

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while(!asyncLoad.isDone)
        {
            if(loading != null) {
                loading.fillAmount = asyncLoad.progress;
            }
            yield return null;
        }
    }

    IEnumerator Fade()
    {
        float time = 0;
        Color color = fadeBackground.color;
        while (time < duration)
        {
            float t = time / duration;
            color.a = Mathf.Lerp(1, 0, t);
            fadeBackground.color = color;
            time += Time.deltaTime;
            yield return null;
        }
    }

}
