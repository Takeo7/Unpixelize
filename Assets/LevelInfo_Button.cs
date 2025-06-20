using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfo_Button : MonoBehaviour
{
    public int lvl_id = 0;
    public bool unlocked = false;
    public bool solved = false;
    [Space]
    public Scene_Controller sc;
    [Space]
    public Button lvl_bt;
    [Space]
    public GameObject unlocked_ui;
    public GameObject locked_ui;
    public GameObject solved_ui;
    [Space]
    public Color unlockedColor;
    public Color lockedColor;

    

    public void LoadLevelID(int id, bool un, bool sol)
    {
        lvl_id = id;
        transform.GetComponentInChildren<TextMeshProUGUI>().text = id.ToString();

        solved = sol;
        unlocked = un;
        UpdateLevel();
    }

    public void UpdateLevel()
    {
        lvl_bt.interactable = unlocked;
        if (solved)
        {
            locked_ui.SetActive(false);
            solved_ui.SetActive(true);
            transform.GetComponentInChildren<TextMeshProUGUI>().color = unlockedColor;
        }
        else if (unlocked)
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
