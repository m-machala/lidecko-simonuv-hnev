using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenManagerScript : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Image LoadingBarFill;
    public AudioClip click;
    public AudioSource audioSource;

    public void LoadScene(string sceneName)
    {
        //SceneManager.LoadScene(sceneName);
        audioSource.PlayOneShot(click);
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void SimpleLoadScene(string sceneName)
    {
        audioSource.PlayOneShot(click);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        audioSource.PlayOneShot(click);
        Application.Quit();
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        LoadingScreen.SetActive(true);
        while (operation != null)
        { 
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            LoadingBarFill.fillAmount = progressValue;

            yield return null;
        }
    }
}
