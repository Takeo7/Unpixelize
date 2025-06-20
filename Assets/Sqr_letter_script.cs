using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Sqr_letter_script : MonoBehaviour
{
    public string letter;
    public bool inEmptySlot = false;

    [Space]
    public Color std;
    public Color up;
    public Color wrong;

    [Space]
    [Header("Managers")]
    MovieGuess_Controller mgc;
    MovieGuess_LettersController mg_lc;

    void Start()
    {
        mgc = MovieGuess_Controller.MovieGuess_instance;
        mg_lc = MovieGuess_LettersController.MovieGuessLetters_instance;
    }

    public void OnClick()
    {

        Debug.Log("Letra " + letter + ". En EmptySlot: " + inEmptySlot);
        if (!inEmptySlot)
        {
            Transform emptySlot = mg_lc.GetNextEmptySlot();
            if (emptySlot != null)
            {
                Debug.Log("Enviada al empty");
                transform.SetParent(emptySlot, false);
                transform.localPosition = Vector3.zero;
                inEmptySlot = true;
                mg_lc.AddTryLetter(this);
                SetUpColor();
                mg_lc.CheckTitle(MovieGuess_Controller.MovieGuess_instance.title);                
            }
        }
        else
        {
            mgc.IsIncorrectTitle(false);
            Transform returnSlot = mg_lc.GetAvailableGridSlot();
            if (returnSlot != null)
            {
                Debug.Log("Enviada al Principal");
                transform.SetParent(returnSlot, false);
                transform.localPosition = Vector3.zero;
                inEmptySlot = false;
                mg_lc.RemoveTryLetter(this);
                if (CheckColor() == wrong)
                {
                    mg_lc.AllTryLettersColorInput(MovieGuess_LettersController.LetterState.Up);
                }
                    SetStdColor();
            }
        }
    }

    public void SetWrongColor()
    {
        transform.GetComponent<Image>().color = wrong;
    }
    public void SetStdColor()
    {
        transform.GetComponent<Image>().color = std;
    }
    public void SetUpColor()
    {
        transform.GetComponent<Image>().color = up;
    }
    public Color CheckColor()
    {
        return transform.GetComponent<Image>().color;
    }
}



