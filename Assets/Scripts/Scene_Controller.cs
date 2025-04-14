using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Controller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
