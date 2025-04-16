using TMPro;
using UnityEngine;

public class Sqr_letter_script : MonoBehaviour
{
    MovieGuess_Controller lc;

    private void Start()
    {
        lc = MovieGuess_Controller.MovieGuess_instance;
    }
    public void LetterClick(GameObject l)
    {
        lc.mg_lc.Grid_Empty_Letter_Squares.transform.GetChild(lc.mg_lc.last_letter_count).GetComponentInChildren<TextMeshProUGUI>().SetText(l.GetComponentInChildren<TextMeshProUGUI>().text);
        lc.mg_lc.last_letter_count++;
        gameObject.SetActive(false);
        lc.mg_lc.CheckTitle(lc.length, lc.title);
        Destroy(gameObject);
    }
}
