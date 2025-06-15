using UnityEngine;
using TMPro;
using System.Collections;

public class Menu_Buttons_Controller : MonoBehaviour
{
    [Space]
    public GameObject level_prefab;
    public Transform level_parent;

    [Space]
    public GameObject loadingScreen;

    [Space]
    public PlayerInfoController pic;
    public PlayerData pd;

    [Space]
    public Scene_Controller sc;

    [Space]
    [Header("Popcorn Text")]
    public AnimatedCounter pop_anim;
    public TextMeshProUGUI pop_text;

    [Space]
    public GameObject dailyReward_GO;
    public TextMeshProUGUI dialyReward_pop_text;
    public float time_daily = 1f;

    void Start()
    {
        pic = PlayerInfoController.Player_Instance;
        pd = pic.playerData;

        ApiClient.Instance.GetLevels(
            onSuccess: response =>
            {
                loadingScreen.SetActive(false);
                LoadLevels();
                sc.PreloadScene(Scene_Controller.Scenes.MovieSelector.ToString());
                pic.LoadPopcornsText(pop_text);
                CheckDailyreward();
            },
            onError: error =>
            {
                Debug.LogError("Error cargando niveles: " + error);
            }

            );

        BugReportingScript.bugInstance.ResetCamera();

    }

    public void LoadLevels()
    {
        int length = pd.levelsProgress.Count;
        Debug.Log("Loaded " + length + " Levels");
        for (int i = 0; i < length; i++)
        {
            GameObject temp = Instantiate(level_prefab, level_parent);
            temp.GetComponent<LevelInfo_Button>().LoadLevelID(pd.levelsProgress[i].levelId, pd.levelsProgress[i].unlocked);
            temp.GetComponent<LevelInfo_Button>().sc = sc;
        }
    }

    public void CheckDailyreward()
    {
        Debug.Log("Daily reward del PIC: " + pic.dailyReward);
        if (pic.dailyReward == true)
        {
            int daily = pic.playerData.daily_reward;
            dailyReward_GO.SetActive(true);
            dialyReward_pop_text.text = daily.ToString();
            pop_anim.SetPoints(daily);
            pic.dailyReward = false;
            StartCoroutine(TimerNotification());
        }
        else
        {
            Debug.Log("No DAILY REWARD");
        }
    }

    IEnumerator TimerNotification()
    {
        yield return new WaitForSecondsRealtime(3f);
        dailyReward_GO.SetActive(false);
    }

   
}
