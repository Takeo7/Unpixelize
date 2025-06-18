using UnityEngine;
using TMPro;
using System.Collections;

public class Movie_Buttons_Controller : MonoBehaviour
{
    [Space]
    public GameObject movie_prefab;
    public Transform movies_parent;

    [Space]
    public GameObject loadingScreen;
    public int loadedImages = 0;

    [Space]
    public PlayerInfoController pic;
    public PlayerData pd;

    [Space]
    public Scene_Controller sc;

    [Space]
    public TextMeshProUGUI level_name;

    [Space]
    [Header("Popcorn Text")]
    public TextMeshProUGUI pop_text;

    void Start()
    {
        pic = PlayerInfoController.Player_Instance;
        pd = pic.playerData;
        level_name.text = "Level " + PlayerInfoController.Player_Instance.currentLevel;

        ApiClient.Instance.GetLevel(PlayerInfoController.Player_Instance.currentLevel,
            onSuccess: response =>
            {
                
                StartCoroutine(LoadMoviesSequentially());
            },
            onError: error =>
            {
                Debug.LogError("Error cargando niveles: " + error);
            }
        );

        pic.LoadPopcornsText(pop_text);

        BugReportingScript.bugInstance.ResetCamera();
    }

    public void MovieLoaded()
    {
        loadedImages++;
        if (loadedImages >= 9)
        {
            LoadingScreenSwitch();
        }
    }

    private IEnumerator LoadMoviesSequentially()
    {
        int currentLevelIndex = PlayerInfoController.Player_Instance.currentLevel - 1;

        if (currentLevelIndex < 0 || currentLevelIndex >= pd.levelsProgress.Count)
        {
            Debug.LogError("Nivel inv�lido: " + currentLevelIndex);
            yield break;
        }

        var levelData = pd.levelsProgress[currentLevelIndex];

        for (int i = 0; i < levelData.subLevels.Count; i++)
        {
            GameObject temp = Instantiate(movie_prefab, movies_parent);
            var button = temp.GetComponent<MovieInfo_Button>();
            button.mbc = this;
            //Debug.Log("ESTE ES EL TITULO QUE APARECE: " + levelData.subLevels[i].film.name.en);
            button.LoadMovieID(
                levelData.subLevels[i].sublevel_id,
                levelData.subLevels[i].solved,
                levelData.subLevels[i].film.name.en);
            button.sc = sc;

            yield return null; // o yield return new WaitForSeconds(0.05f); para suavizar m�s
        }

        
    }

    public void LoadingScreenSwitch()
    {
        loadingScreen.SetActive(false);
    }

    public void BackScene()
    {
        sc.ChangeScene(Scene_Controller.Scenes.LevelSelector);
    }
}
