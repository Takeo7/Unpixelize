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
    [Space]
    public GameObject unlocked_ui;
    public GameObject locked_ui;
    [Space]
    public Color unlockedColor;
    public Color lockedColor;

    

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
        if (unlocked)
        {
            locked_ui.SetActive(false);
            unlocked_ui.SetActive(true);
            transform.GetComponentInChildren<TextMeshProUGUI>().color = unlockedColor;
        }
        else
        {
            locked_ui.SetActive(true);
            unlocked_ui.SetActive(false);
            transform.GetComponentInChildren<TextMeshProUGUI>().color = lockedColor;
        }
    }

    void OnEnable()
    {
        if (unlocked)
        {
            sc.PreloadScene(Scene_Controller.Scenes.MovieSelector.ToString());
        }
    }

    public void EnterLevel()
    {
        PlayerInfoController.Player_Instance.currentLevel = lvl_id;
        sc.ActivatePreloadedScene();
    }

}
