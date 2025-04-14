using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfo_Button : MonoBehaviour
{
    public int lvl_id = 0;
    public bool unlocked = false;
    [Space]
    public Scene_Controller sc;
    [Space]
    public Button lvl_bt;

    public void LoadLevelID(int id, bool un)
    {
        lvl_id = id;
        transform.GetComponentInChildren<TextMeshProUGUI>().text = id.ToString();

        unlocked = un;
        UpdateLevel();
    }

    public void UpdateLevel()
    {
        lvl_bt.interactable = unlocked;
    }

    public void EnterLevel()
    {
        PlayerInfoController.Player_Instance.currentLevel = lvl_id;
        
        sc.ChangeScene(Scene_Controller.Scenes.MovieSelector);
    }
}
