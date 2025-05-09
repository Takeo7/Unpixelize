using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Controller : MonoBehaviour
{

    private AsyncOperation preloadOperation;

    public void PreloadScene(string sceneName)
    {
        if (preloadOperation == null)
        {
            preloadOperation = SceneManager.LoadSceneAsync(sceneName);
            preloadOperation.allowSceneActivation = false;
            Debug.Log($"🔄 Precargando escena: {sceneName}");
        }
    }

    public void ActivatePreloadedScene()
    {
        if (preloadOperation != null)
        {
            preloadOperation.allowSceneActivation = true;
            preloadOperation = null;
            Debug.Log("✅ Activando escena precargada");
        }
    }

    public void ChangeScene(Scenes scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    public enum Scenes
    {
        MainMenu,
        LevelSelector,
        MovieSelector,
        MovieGuess
    }
}
