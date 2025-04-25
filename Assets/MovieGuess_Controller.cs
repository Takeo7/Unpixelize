using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

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
    public Scene_Controller sc;
    [Space]

    [Space]
    public TitleLanguage tit_lang;
    public string title;
    public int length;
    public int pixelice;

    [Space]
    int fakeLetters = 4;
    [Space]
    public GameObject correct_img;
    public GameObject wrong_img;

    private void Start()
    {
        pic = PlayerInfoController.Player_Instance;
        pixelice = 15;
    }

    public enum TitleLanguage
    {
        es,
        en
    }

    public void ChangeLanguage()
    {
        switch (tit_lang)
        {
            case TitleLanguage.es:
                tit_lang = TitleLanguage.en;
                break;
            case TitleLanguage.en:
                tit_lang = TitleLanguage.es;
                break;
            default:
                break;
        }

        SetMovieData();
    }
    public void SetMovieData()
    {
        CleanTitle();
        mg_lc.SetEmptySquares(length);
        mg_lc.SetLetterSquares(length, title, fakeLetters);

        //Video Controller

        mg_vc.SetPixelice(pixelice);
    }

    public void CleanTitle()
    {
        switch (tit_lang)
        {
            case TitleLanguage.es:
                title = pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.name.es;
                break;
            case TitleLanguage.en:
                title = pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.name.en;
                break;
            default:
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

    #region PowerButtons

    public void AddNewLetter()
    {
        mg_lc.AutoPlaceNextCorrectLetter(title);
    }

    #endregion

    public void BackButton()
    {
        mg_vc.SetPixelice(15);
        sc.ChangeScene(Scene_Controller.Scenes.MovieSelector);
    }

}
