using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MovieGuess_Controller : MonoBehaviour
{
    #region Sigleton
    public static MovieGuess_Controller MovieGuess_instance { get; private set; }

    private void Awake()
    {
        if (MovieGuess_instance == null)
        {
            MovieGuess_instance = this;
            //DontDestroyOnLoad(gameObject); // Opcional: Mantiene la instancia entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe una instancia, destruye la nueva
        }
    }
    #endregion

    [Space]
    public PlayerInfoController pic;
    [Space]
    public Movie_Selector ms;
    [Space]
    public MovieGuess_VideoController mg_vc;
    [Space]
    public MovieGuess_LettersController mg_lc;
    [Space]

    [Space]
    public TitleLanguage tit_lang;
    public string title;
    public int length;

    [Space]
    public GameObject correct_img;
    public GameObject wrong_img;

    private void Start()
    {
        pic = PlayerInfoController.Player_Instance;
    }

    public enum TitleLanguage
    {
        es,
        en
    }

    public void SetMovieData()
    {
        CleanTitle();
        mg_lc.SetEmptySquares(length);
        mg_lc.SetLetterSquares(length, title);

        //Video Controller

        mg_vc.SetPixelice(15);
    }

    public void CleanTitle()
    {
        switch (tit_lang)
        {
            case TitleLanguage.es:
                title = pic.playerData.levelsProgress[pic.currentLevel].subLevels[pic.currentMovie].film.names.es;
                break;
            case TitleLanguage.en:
                title = pic.playerData.levelsProgress[pic.currentLevel].subLevels[pic.currentMovie].film.names.en;
                break;
            default:
                title = pic.playerData.levelsProgress[pic.currentLevel].subLevels[pic.currentMovie].film.names.en;
                break;
        }

        title = RemoveVoids(title);

        length = Calculatelength(title);
    }

    public string RemoveVoids(string s)
    {
        int length = s.Length;

        string s_final = s.Replace(" ", "");

        return s_final;
    }

    public int Calculatelength(string s)
    {
        int count = s.Length;

        return count;
    }

    public void BackButton()
    {
        mg_vc.SetPixelice(15);
        SceneManager.LoadScene(1);
    }

}
