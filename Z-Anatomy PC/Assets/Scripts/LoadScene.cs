using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    public float duration;
    public int sceneIndex;
    public Image fadeBackground, loading;
    
    private IEnumerator Start()
    {
        if(loading != null) {
            loading.fillAmount = 0f;
        }

        if(fadeBackground != null) {
            yield return StartCoroutine(Fade());
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
