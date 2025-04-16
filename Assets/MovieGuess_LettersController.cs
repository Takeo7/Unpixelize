using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MovieGuess_LettersController : MonoBehaviour
{

    [Space]
    public GameObject Grid_Empty_Letter_Squares;
    public GameObject empty_letter_square;
    public int last_letter_count = 0;

    [Space]
    public GameObject Grid_Letter_Squares;
    public GameObject letter_square;

    public void SetEmptySquares(int length)
    {
        for (int i = 0; i < length; i++)
        {
            GameObject g = Instantiate(empty_letter_square);
            g.transform.SetParent(Grid_Empty_Letter_Squares.transform);
        }
    }

    public void SetLetterSquares(int length, string title)
    {
        GameObject[] letters_t = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            GameObject g = Instantiate(letter_square);
            g.transform.GetComponentInChildren<TextMeshProUGUI>().SetText(title[i].ToString().ToUpper());
            FillArrayRandomly(letters_t, g);
        }

        for (int i = 0; i < length; i++)
        {
            letters_t[i].transform.SetParent(Grid_Letter_Squares.transform);
            letters_t[i].SetActive(true);
        }
    }

    public void FillArrayRandomly(GameObject[] letters_t, GameObject g)
    {
        List<int> emptyIndices = new List<int>();

        // Guardar los índices vacíos
        for (int i = 0; i < letters_t.Length; i++)
        {
            if (letters_t[i] == null)
            {
                emptyIndices.Add(i);
            }
        }

        // Si hay huecos disponibles, elegir uno aleatorio
        if (emptyIndices.Count > 0)
        {
            int randomIndex = emptyIndices[Random.Range(0, emptyIndices.Count)];
            letters_t[randomIndex] = g;
            Debug.Log("Nuevo weon dentro en posición " + randomIndex);
        }
        else
        {
            Debug.Log("No hay huecos disponibles.");
        }
    }

    public void DeleteLetter()
    {
        if (last_letter_count > 0)
        {
            Transform lastLetter_t = Grid_Empty_Letter_Squares.transform.GetChild(last_letter_count - 1);
            string lastLetter = lastLetter_t.GetComponentInChildren<TextMeshProUGUI>().text;

            lastLetter_t.GetComponentInChildren<TextMeshProUGUI>().text = "";
            last_letter_count--;

            GameObject newLetterSquare = Instantiate(letter_square);
            newLetterSquare.transform.GetComponentInChildren<TextMeshProUGUI>().text = lastLetter;
            newLetterSquare.transform.SetParent(Grid_Letter_Squares.transform);
            newLetterSquare.SetActive(true);
        }
        else
        {
            Debug.Log("There is no letter placed");
        }

    }

    public bool CheckTitle(int length, string title)
    {
        if (last_letter_count == length)
        {
            string check_title = "";
            for (int i = 0; i < last_letter_count; i++)
            {
                check_title += Grid_Empty_Letter_Squares.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text;
            }

            if (check_title.ToUpper() == title.ToUpper())
            {
                Debug.Log("Title check: IS CORRECT");
                return true;
            }
            else
            {
                Debug.Log("Title check: IS WRONG");
                return false;
            }
        }
        else
        {
            Debug.Log("ERROR: no cuenta bien las letras");
            return false;
        }
    }
}
