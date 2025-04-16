using UnityEngine;
using TMPro;

public class Movie_Buttons_Controller : MonoBehaviour
{
    [Space]
    public GameObject movie_prefab;
    public Transform movies_parent;

    [Space]
    public PlayerData pd;

    [Space]
    public Scene_Controller sc;

    [Space]
    public TextMeshProUGUI level_name;

    void Start()
    {
        pd = PlayerInfoController.Player_Instance.playerData;
        level_name.text = "Level " + PlayerInfoController.Player_Instance.currentLevel;
        LoadMovies();
    }

    public void LoadMovies()
    {
        int length = 9;
        
        for (int i = 0; i < length; i++)
        {
            GameObject temp = Instantiate(movie_prefab, movies_parent);
            temp.GetComponent<MovieInfo_Button>().LoadMovieID(
                pd.levelsProgress[PlayerInfoController.Player_Instance.currentLevel].subLevels[i].sublevel_id,
                pd.levelsProgress[PlayerInfoController.Player_Instance.currentLevel].subLevels[i].solved,
                pd.levelsProgress[PlayerInfoController.Player_Instance.currentLevel].subLevels[i].film.path_to_photo);
            temp.GetComponent<MovieInfo_Button>().sc = sc;
        }
    }
}
