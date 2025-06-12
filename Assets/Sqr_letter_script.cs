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

    public void OnClick()
    {
        MovieGuess_Controller.MovieGuess_instance.IsIncorrectTitle(false);
        Debug.Log("Letra " + letter + ". En EmptySlot: "+inEmptySlot);
        if (!inEmptySlot)
        {
            Transform emptySlot = MovieGuess_LettersController.MovieGuessLetters_instance.GetNextEmptySlot();
            if (emptySlot != null)
            {
                Debug.Log("Enviada al empty");
                transform.SetParent(emptySlot, false);
                transform.localPosition = Vector3.zero;
                inEmptySlot = true;
                MovieGuess_LettersController.MovieGuessLetters_instance.CheckTitle(MovieGuess_Controller.MovieGuess_instance.title);
                transform.GetComponent<Image>().color = up;
            }
        }
        else
        {
            Transform returnSlot = MovieGuess_LettersController.MovieGuessLetters_instance.GetAvailableGridSlot();
            if (returnSlot != null)
            {
                Debug.Log("Enviada al Principal");
                transform.SetParent(returnSlot, false);
                transform.localPosition = Vector3.zero;
                inEmptySlot = false;
                transform.GetComponent<Image>().color = std;
            }
        }
    }
}



