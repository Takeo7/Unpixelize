using TMPro;
using UnityEngine;

public class Sqr_letter_script : MonoBehaviour
{
    Level_Controller lc;

    private void Start()
    {
        lc = Level_Controller.Instance;
    }
    public void LetterClick(GameObject l)
    {
        lc.Grid_Empty_Letter_Squares.transform.GetChild(lc.last_letter_count).GetComponentInChildren<TextMeshProUGUI>().SetText(l.GetComponentInChildren<TextMeshProUGUI>().text);
        lc.last_letter_count++;
        gameObject.SetActive(false);
        lc.CheckTitle();
        Destroy(gameObject);
    }
}
