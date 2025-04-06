using UnityEngine;

public class Menu_Buttons_Controller : MonoBehaviour
{
    [Space]
    public GameObject level_prefab;
    public Transform level_parent;

    [Space]
    public PlayerData pd;

    void Start()
    {
        
    }

    public void LoadLevels()
    {
        int length = pd.levelsProgress.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject temp = Instantiate(level_prefab, level_parent);
            temp.GetComponent<LevelInfo_Button>().LoadLevelID(pd.levelsProgress[i].levelId);
        }
    }

   
}
