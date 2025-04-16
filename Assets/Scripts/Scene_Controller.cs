using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Controller : MonoBehaviour
{

    public void ChangeScene(Scenes scene)
    {
        SceneManager.LoadScene(scene.ToString());

    }

    public enum Scenes
    {
        MainMenu,
        LevelSelector,
        MovieSelector,
        MovieGuess
    }
}
