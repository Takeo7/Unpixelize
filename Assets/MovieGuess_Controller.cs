using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

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
    public GameObject[] itemsToHideSolvedView;

    [Space]
    [Header("Power Buttons")]
    public GameObject powerButtons;
    public Button PX_Butt;
    public UnpixeliceAnimation unpix_button_anim;
    public Button TNT_Butt;
    public Image tnt_img;

    [Space]
    public List<TextMeshProUGUI> tips_text = new List<TextMeshProUGUI>();

    [Space]
    public GameObject loading_screen;
    public GameObject softLoadingScreen;

    [Space]
    public ApiClient _api;

    [Space]
    [Header("Popcorns")]
    public TextMeshProUGUI pop_text;
    public AnimatedCounter pop_anim;
    [Space]
    public TextMeshProUGUI letterPrice_text;
    public TextMeshProUGUI tntPrice_text;
    public TextMeshProUGUI PXPrice_text;
    public TextMeshProUGUI TipsPrice_text;
    public TextMeshProUGUI KeyPrice_text;
    [Space]
    public GameObject poor_msg;

    [Space]
    [Header("Reward popup")]
    public GameObject NotificationsMain_GO;
    public Transform NotificationGrid;
    public GameObject Notification_GO;



    private void Start()
    {
        pic = PlayerInfoController.Player_Instance;
        pixelice = 15 + (pic.GetCurrentMovieData().help.help_pixel.pixel_count * 10);
        SetMovieData();

        LoadPopcornsText_mgc();
        LoadHelpersTexts();

        BugReportingScript.bugInstance.ResetCamera();
    }


    public void LoadPopcornsText_mgc()
    {
        pic.LoadPopcornsText(pop_text);
    }
    public void LoadHelpersTexts()
    {
        pic.LoadHelpersText(letterPrice_text, PlayerInfoController.Purchase_Type.newLetter);
        pic.LoadHelpersText(tntPrice_text, PlayerInfoController.Purchase_Type.tnt);
        pic.LoadHelpersText(PXPrice_text, PlayerInfoController.Purchase_Type.pixel);
        pic.LoadHelpersText(TipsPrice_text, PlayerInfoController.Purchase_Type.clue);
        pic.LoadHelpersText(KeyPrice_text, PlayerInfoController.Purchase_Type.key);

        CheckUnpixeliceButton();
        CheckTNTButton();
    }

    public void CantPurchase()
    {
        poor_msg.SetActive(true);
        StartCoroutine(Cantbuy_Coroutine());
        //Debug.Log("This can not be purchased... poor scum");
    }

    private IEnumerator Cantbuy_Coroutine()
    {
        yield return new WaitForSecondsRealtime(3f);
        poor_msg.SetActive(false);

    }


    #region Language

    public enum TitleLanguage
    {
        es,
        en
    }

    public void ChangeLanguage(TitleLanguage lang)
    {
        ShowSoftLoadingScreen();
        IsIncorrectTitle(false);
        tit_lang = lang;

        UpdateMovieData();
    }

    #endregion

    #region Loading Screen

    public void ShowSoftLoadingScreen()
    {
        softLoadingScreen.SetActive(true);
    }

    public void HideSoftLoadingScreen()
    {
        softLoadingScreen.SetActive(false);
    }

    #endregion


    public void SetMovieData()
    {

        _api = ApiClient.Instance;

        //Get Helpers

        _api.GetHelpData(pic.currentLevel, pic.currentMovie,
            onSuccess: response =>
            {
                IsIncorrectTitle(false);
                //Debug.Log("SetMovieData");
                CleanTitle();
                mg_lc.SetAllLetters(length, length, title, fakeLetters);
                SetTips();

                SetVideo();
                mg_vc.SetPixelice(pixelice);
                //Debug.Log("Traza LC");
                loading_screen.SetActive(false); 
            },
             onError: err => {
                 Debug.LogError("Get Help Data ERROR");
             });

        
    }

    public void UpdateMovieData()
    {

        _api = ApiClient.Instance;

        //Get Helpers

        _api.GetHelpData(pic.currentLevel, pic.currentMovie,
            onSuccess: response =>
            {
                IsIncorrectTitle(false);
                //Debug.Log("SetMovieData");
                CleanTitle();
                mg_lc.SetAllLetters(length, length, title, fakeLetters);
                CheckTNTButton();
            },
             onError: err => {
                 Debug.LogWarning("Get Help Data ERROR");
             });


    }
    public void SetVideo()
    {
        mg_vc.PlayVideoFromAddressables("Videos/"+ pic.GetCurrentMovieData().film.name.en);
    }
    public void CleanTitle()
    {
        switch (tit_lang)
        {
            case TitleLanguage.es:
                title = pic.GetCurrentMovieData().film.name.es;
                break;
            case TitleLanguage.en:
                title = pic.GetCurrentMovieData().film.name.en;
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


    #region SolvedLevel

    public void IsCorrectTitle(bool y)
    {
        ShowSoftLoadingScreen();

        correct_img.gameObject.SetActive(true);

        if (y)
        {
            //Post Completed
            _api.MarkSubLevelSolved(pic.currentLevel, pic.currentMovie,
                onSuccess: (SolvedSublevelResponse response) =>
                {
                    pic.SetPopcorns(PlayerInfoController.Win_Type.movie_solved, pop_anim);
                    //LoadPopcornsText_mgc();
                    PrepareSolvedView();
                    //Debug.Log("Post Correct Movie SUCCESS: " + response);
                    HideSoftLoadingScreen();



                    ShowRewardPopup("Movie Solved", pic.win_amount[0].ToString());

                    if (response.level_completed)
                    {
                        //Debug.Log("🎉 Nivel completo!");
                        ShowRewardPopup("Level completed", pic.win_amount[1].ToString());
                    }

                    if (response.next_level_unlocked)
                    {
                        //Debug.Log("🔓 ¡Nuevo nivel desbloqueado!");
                        ShowRewardPopup("Next level unlocked");
                    }
                },
                onError: error =>
                {
                    HideSoftLoadingScreen();
                    Debug.LogError("Post Correct Movie fallido: " + error);

                });
        }
        

        mg_lc.DisableEmptyLetterSquares();

        PrepareSolvedView();
        //correctTextPlatform.text = pic.playerData.levelsProgress[pic.currentLevel-1].subLevels[pic.currentMovie-1].film.platform;
        correctTextPlatform.text = "PRIME VIDEO";
        powerButtons.SetActive(false);

        mg_lc.AutoPlaceAllCorrectLetters(title);

        mg_vc.SetPixelice(55);
    }

    #region Notifications
    public void ShowRewardPopup(string message, string amount)
    {
        GameObject noti = Instantiate(Notification_GO, NotificationGrid);
        NotificationPopupScript noti_Script = noti.GetComponent<NotificationPopupScript>();
        noti_Script.message_text.text = message;
        noti_Script.Price_text.text = amount;
        noti_Script.popcorns_go.SetActive(true);
        NotificationsMain_GO.SetActive(true);
        StartCoroutine(TimerNotifications(noti));
    }

    public void ShowRewardPopup(string message)
    {
        GameObject noti = Instantiate(Notification_GO, NotificationGrid);
        NotificationPopupScript noti_Script = noti.GetComponent<NotificationPopupScript>();
        noti_Script.message_text.text = message;
        noti_Script.popcorns_go.SetActive(false);
        NotificationsMain_GO.SetActive(true);
        StartCoroutine(TimerNotifications(noti));
    }

    public void ClearNotifications()
    {
        foreach (Transform childen in NotificationGrid.GetComponentsInChildren<Transform>())
        {
            Destroy(childen.gameObject);
        }
    }

    IEnumerator TimerNotifications(GameObject go)
    {
        yield return new WaitForSecondsRealtime(3f);
        Destroy(go);
    }
#endregion
    public void PrepareSolvedView()
    {
        correctView.SetActive(true);
        foreach (GameObject item in itemsToHideSolvedView)
        {
            item.SetActive(false);
        }
    }

    public void CheckIsAlreadySolved()
    {
        if (pic.GetCurrentMovieData().solved)
        {
            IsCorrectTitle(false);
            HideSoftLoadingScreen();
        }
    }

    public void IsIncorrectTitle(bool n)
    {
        wrong_img.gameObject.SetActive(n);
    }

    #endregion

    #region PowerButtons

    #region Add New Letter
    public void AddNewLetter()
    {
        _api = ApiClient.Instance;

        //Get Helpers

        _api.UseHelpLetter(pic.currentLevel, pic.currentMovie, tit_lang.ToString(),
            onSuccess: response =>
            {
                if (pic.SetPopcorns(PlayerInfoController.Purchase_Type.newLetter, pop_anim))
                {
                    mg_lc.AutoPlaceNextCorrectLetter(title);
                    //LoadPopcornsText_mgc();
                }
                else
                {
                    CantPurchase();
                }
                HideSoftLoadingScreen();
            },
             onError: err => {
                 if (err == "poor")
                 {
                     CantPurchase();
                 }
                 Debug.Log("Get Help New Letter ERROR");
                 HideSoftLoadingScreen();
             });

        

    }

    public void AddNewLetter_back(int index, string letter, TitleLanguage lang)
    {
        Debug.LogWarning("Falta llamada de Add new letter a la API");
    }
    #endregion

    #region Autosolve
    public void AutoSolveMovie()
    {

        PostAutosolveMovie();
        
    }

    public void PostAutosolveMovie()
    {
        _api.UseHelpKey(pic.currentLevel, pic.currentMovie,
            onSuccess: response =>
            {
                if (pic.SetPopcorns(PlayerInfoController.Purchase_Type.key, pop_anim))
                {
                    //LoadPopcornsText_mgc();
                    mg_lc.AutoPlaceAllCorrectLetters(title);
                    Debug.Log("Post Key Helper Movie SUCCESS: " + response);
                    IsCorrectTitle(false);
                }
                else
                {
                    CantPurchase();
                    HideSoftLoadingScreen();
                }
                
                HideSoftLoadingScreen();
            },
                onError: error =>
                {
                    if (error == "poor")
                 {
                     CantPurchase();
                 }
                    Debug.LogError("Post Key Helper Movie fallido: " + error);
                    HideSoftLoadingScreen();

                });
        
    }

    #endregion

    #region Tips
    public void SetTips()
    {
        List<string> movieClues = new List<string>();
        List<string> clueType = new List<string>();
        movieClues.Add(pic.GetCurrentMovieData().film.actor);
        clueType.Add("cast");
        movieClues.Add(pic.GetCurrentMovieData().film.director);
        clueType.Add("director");
        movieClues.Add(pic.GetCurrentMovieData().film.year.ToString());
        clueType.Add("year");

        foreach (var item in tips_text)
        {
            int rand = Random.Range(0, movieClues.Count);
            item.text = movieClues[rand];
            item.transform.parent.GetChild(1).GetComponent<Tip_info>().tip_Type = clueType[rand];
            if (pic.GetCurrentMovieData().help.help_clues != null)
            {
                for (int i = 0; i < pic.GetCurrentMovieData().help.help_clues.Count; i++)
                {
                    if (pic.GetCurrentMovieData().help.help_clues[i].type == clueType[rand])
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
            if (pic.SetPopcorns(PlayerInfoController.Purchase_Type.clue, pop_anim))
                {
                ti.gameObject.SetActive(false);
                Debug.Log("Post Clue " + ti.tip_Type + " Movie SUCCESS: " + response);
                HideSoftLoadingScreen();
                }
            else
                {
                    CantPurchase();
                    HideSoftLoadingScreen();
                }
            },
               onError: error =>
               {
                    if (error == "poor")
                 {
                     CantPurchase();
                 }
                   Debug.LogError("Post Clue " + ti.tip_Type + " Movie fallido: " + error);
                   HideSoftLoadingScreen();

               });
            //LoadPopcornsText_mgc();
        

        

    }
    #endregion

    #region TNT

    public void TntFakeWords()
    {
        
            _api.UseHelpBomb(pic.currentLevel, pic.currentMovie,
            onSuccess: response =>
            {
            if (pic.SetPopcorns(PlayerInfoController.Purchase_Type.tnt, pop_anim))
                {
                mg_lc.EliminarLetrasFalsas();
                //LoadPopcornsText_mgc();
                pic.GetCurrentMovieData().help.help_bombs.id = 1;
                CheckTNTButton();
                Debug.Log("Post TNT Movie SUCCESS: " + response);
                }
            else
                {
                CantPurchase();
                HideSoftLoadingScreen();
                }
            },
                onError: error =>
                {
                    if (error == "poor")
                 {
                     CantPurchase();
                 }
                    Debug.LogError("Post TNT Movie fallido: " + error);

                });
            
            HideSoftLoadingScreen();
        

        
    }

    public void CheckTNTButton()
    {
        if (pic.GetCurrentMovieData().help.help_bombs.id != 0)
        {
            TNT_Butt.interactable = false;
            tnt_img.color = Color.grey;
            mg_lc.EliminarLetrasFalsas();
        }
    }

    #endregion

    #region Unpixelice

    public void Unpixelice()
    {
        if (pic.GetCurrentMovieData().help.help_pixel.pixel_count < pic.playerData.px_limit)
        {
            ApiClient.Instance.UseHelpPixel(pic.currentLevel, pic.currentMovie,
            onSuccess: info => {
                if (pic.SetPopcorns(PlayerInfoController.Purchase_Type.pixel, pop_anim))
                {
                    pic.GetCurrentMovieData().help.help_pixel.pixel_count++;
                    pixelice += 10;
                    mg_vc.SetPixelice(pixelice);

                    //LoadPopcornsText_mgc();
                    CheckUnpixeliceButton();
                }
                else
                {
                    CantPurchase();
                }
                HideSoftLoadingScreen();
            },
             onError: err => {
                if (err == "poor")
                 {
                     CantPurchase();
                 }
                 CantPurchase();
                 HideSoftLoadingScreen();
             });
        }

        

    }

    public void CheckUnpixeliceButton()
    {
        int lvlarr = pic.currentLevel - 1;
        int moviearr = (pic.currentMovie - 1) - (9 * (pic.currentLevel - 1));
        Debug.Log("Tring to access ->>>> LEVEL array: " + lvlarr + " &&& MOVIE array: " + moviearr);

        if (pic.GetCurrentMovieData().help.help_pixel.pixel_count >= pic.playerData.px_limit - 1)
        {
            PX_Butt.interactable = false;
        }
        
        unpix_button_anim.Unpixelice(pic.GetCurrentMovieData().help.help_pixel.pixel_count);
    }

    #endregion

    #endregion

    public void OpenReferralLink()
    {
        pic.OpenReferral();
    }

    public void BackButton()
    {
        mg_vc.SetPixelice(15);
        sc.ChangeScene(Scene_Controller.Scenes.MovieSelector);
    }

}
