using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Controller : MonoBehaviour
{

    public void ChangeScene(Scenes scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void PrepareScene(Scenes scene)
    {
        SceneManager.LoadSceneAsync(scene.ToString());
    }

    public enum Scenes
    {
        MainMenu,
        LevelSelector,
        MovieSelector,
        MovieGuess
    }
}
