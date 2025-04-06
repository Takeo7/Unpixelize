using UnityEngine;
using TMPro;

public class LevelInfo_Button : MonoBehaviour
{
    public int lvl_id = 0;
    public bool unlocked = false;
    public bool completed = false;
    [Space]
    public Scene_Controller sc;

    public void LoadLevelID(int id)//, bool un, bool com)
    {
        lvl_id = id;
        transform.GetComponentInChildren<TextMeshProUGUI>().text = id.ToString();

        UpdateLevel();
    }

    public void UpdateLevel()
    {

    }

    public void EnterLevel()
    {
        PlayerInfoController.Player_Instance.currentLevel = lvl_id;
        sc.ChangeScene(3);
    }
}
