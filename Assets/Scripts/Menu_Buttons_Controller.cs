using UnityEngine;

public class Menu_Buttons_Controller : MonoBehaviour
{
    [Space]
    public GameObject level_prefab;
    public Transform level_parent;

    [Space]
    public GameObject loadingScreen;

    [Space]
    public PlayerData pd;

    [Space]
    public Scene_Controller sc;

    void Start()
    {
        pd = PlayerInfoController.Player_Instance.playerData;

        ApiClient.Instance.GetLevels(
            onSuccess: response =>
            {
                loadingScreen.SetActive(false);
                LoadLevels();
            },
            onError: error =>
            {
                Debug.LogError("Error cargando niveles: " + error);
            }

            );

        
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

   
}
