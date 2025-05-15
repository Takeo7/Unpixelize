using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Space]
    public GameObject correctView;
    public TextMeshProUGUI correctTextPlatform;

    [Space]
    public GameObject powerButtons;

    [Space]
    public List<TextMeshProUGUI> tips_text = new List<TextMeshProUGUI>();

    [Space]
    public GameObject loading_screen;

    [Space]
    public ApiClient _api;

    private void Start()
    {
        pic = PlayerInfoController.Player_Instance;
        pixelice = 15;
        SetMovieData();
        
       
    }

    public enum TitleLanguage
    {
        es,
        en
    }

    public void ChangeLanguage()
    {
        IsIncorrectTitle(false);
        switch (tit_lang)
        {
            case TitleLanguage.es:
                tit_lang = TitleLanguage.en;
                break;
            case TitleLanguage.en:
                tit_lang = TitleLanguage.es;
                break;
        }

        SetMovieData();
    }
    public void SetMovieData()
    {

        _api = ApiClient.Instance;

        //Get Helpers

        IsIncorrectTitle(false);
        Debug.Log("SetMovieData");
        CleanTitle();
        mg_lc.SetAllLetters(length, length, title, fakeLetters);
        SetTips();

        SetVideo();
        mg_vc.SetPixelice(pixelice);
        Debug.Log("Traza LC");
        loading_screen.SetActive(false);
    }
    public void SetVideo()
    {
        mg_vc.PlayVideoFromAddressables("Videos/"+RemoveVoids(pic.playerData.levelsProgress[pic.currentLevel - 1].subLevels[pic.currentMovie - 1].film.name.en).ToLower());
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

        //title = RemoveVoids(title);

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

    public void IsCorrectTitle(bool y)
    {
        correct_img.gameObject.SetActive(true);

        if (y)
        {
            //Post Completed
            _api.MarkSubLevelSolved(pic.currentLevel, pic.currentMovie,
                onSuccess: response =>
                {
                    Debug.Log("Post Correct Movie SUCCESS: " + response);
                },
                onError: error =>
                {
                    Debug.LogError("Post Correct Movie fallido: " + error);

                });
        }
        else
        {
            
        }
        

        mg_lc.DisableEmptyLetterSquares();

        correctView.SetActive(true);
        correctTextPlatform.text = pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.platform;

        powerButtons.SetActive(false);

        mg_lc.AutoPlaceAllCorrectLetters(title);
    }

    public void CheckIsAlreadySolved()
    {
        if (pic.playerData.levelsProgress[pic.currentLevel - 1].subLevels[pic.currentMovie - 1].solved)
        {
            IsCorrectTitle(false);
        }
    }

    public void IsIncorrectTitle(bool n)
    {
        wrong_img.gameObject.SetActive(n);
    }

    #region PowerButtons

    #region Add New Letter
    public void AddNewLetter()
    {
        mg_lc.AutoPlaceNextCorrectLetter(title);
    }

    public void PostAddNewLetter_API(int index, string letter, TitleLanguage lang)
    {
        Debug.LogWarning("Falta llamada de Add new letter a la API");
    }
    #endregion

    #region Key Helper
    public void AutoSolveMovie()
    {
        mg_lc.AutoPlaceAllCorrectLetters(title);
        IsCorrectTitle(true);
        PostAutosolveMovie();
    }

    public void PostAutosolveMovie()
    {
        _api.UseHelpKey(pic.currentLevel, pic.currentMovie,
            onSuccess: response =>
            {
                Debug.Log("Post Key Helper Movie SUCCESS: " + response);
            },
                onError: error =>
                {
                    Debug.LogError("Post Key Helper Movie fallido: " + error);

                });
    }

    #endregion

    #region Tips
    public void SetTips()
    {
        List<string> movieClues = new List<string>();
        List<string> clueType = new List<string>();
        movieClues.Add(pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.actor);
        clueType.Add("actor");
        movieClues.Add(pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.director);
        clueType.Add("director");
        movieClues.Add(pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.year.ToString());
        clueType.Add("year");

        foreach (var item in tips_text)
        {
            int rand = Random.Range(0, movieClues.Count);
            item.text = movieClues[rand];
            item.transform.parent.GetChild(1).GetComponent<Tip_info>().tip_Type = clueType[rand];
            if (pic.playerData.levelsProgress[pic.currentLevel - 1].subLevels[pic.currentMovie - 1].help.help_clues != null)
            {
                for (int i = 0; i < pic.playerData.levelsProgress[pic.currentLevel - 1].subLevels[pic.currentMovie - 1].help.help_clues.Count; i++)
                {
                    if (pic.playerData.levelsProgress[pic.currentLevel - 1].subLevels[pic.currentMovie - 1].help.help_clues[i].type == clueType[rand])
                    {
                        item.transform.parent.GetChild(1).gameObject.SetActive(false);
                    }
                }
            }
                      
            movieClues.RemoveAt(rand);
            clueType.RemoveAt(rand);
        }
    }


    public void BuyTip(Tip_info ti)
    {
        _api.UseHelpClue(pic.currentLevel, pic.currentMovie, ti.tip_Type,
             onSuccess: response =>
             {
                 Debug.Log("Post Clue "+ti.tip_Type+" Movie SUCCESS: " + response);
             },
                onError: error =>
                {
                    Debug.LogError("Post Clue " + ti.tip_Type + " Movie fallido: " + error);

                });
    }
    #endregion

    #region TNT

    //Lo hace MG_LC

    #endregion

    #region Unpixelice

    public void Unpixelice()
    {
        mg_vc.UnPixelice(10);
    }

    #endregion

    #endregion

    public void BackButton()
    {
        mg_vc.SetPixelice(15);
        sc.ChangeScene(Scene_Controller.Scenes.MovieSelector);
    }

}
