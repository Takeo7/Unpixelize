using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Level_Controller : MonoBehaviour
{

    public static Level_Controller Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Opcional: Mantiene la instancia entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe una instancia, destruye la nueva
        }
    }

    [Space]
    public Movie_Selector ms;

    [Space]
    public string title;
    public int length;

    [Space]
    public GameObject Grid_Empty_Letter_Squares;
    public GameObject empty_letter_square;
    public int last_letter_count = 0;

    [Space]
    public GameObject Grid_Letter_Squares;
    public GameObject letter_square;

    [Space]
    public Image movie_img;

    [Space]
    public GameObject correct_img;
    public GameObject wrong_img;
    public GameObject deleteAll_Button;

    public void SetMovieData()
    {
        title = ms.movieName;
        
        CleanTitle();
        SetEmptySquares();
        SetLetterSquares();
        SetPixelice(15);
    }

    public void CleanTitle()
    {
        title = RemoveVoids(title);

        length = Calculatelength(title);
    }

    public string RemoveVoids(string s)
    {
        int length = s.Length;

        string s_final = s.Replace(" ", "");

        return s_final;
    }

    public int Calculatelength(string s)
    {
        int count = s.Length;

        return count;
    }

    public void SetEmptySquares()
    {
        for (int i = 0; i < length; i++)
        {
            GameObject g = Instantiate(empty_letter_square);
            g.transform.SetParent(Grid_Empty_Letter_Squares.transform);
        }
    }

    public void SetLetterSquares()
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


    public void UnPixelice(int incremental)
    {
        float pixelsize = movie_img.material.GetFloat("_PixelSize");
        movie_img.material.SetFloat("_PixelSize", pixelsize + incremental);
    }

    public void SetPixelice(int p)
    {
        movie_img.material.SetFloat("_PixelSize", p);
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

        ResetResult();

    }

    public void DeleteAllLetters()
    {
        for (int i = 0; i < last_letter_count; i++)
        {
            if (last_letter_count > 0)
            {
                Transform lastLetter_t = Grid_Empty_Letter_Squares.transform.GetChild(i);
                string lastLetter = lastLetter_t.GetComponentInChildren<TextMeshProUGUI>().text;

                lastLetter_t.GetComponentInChildren<TextMeshProUGUI>().text = "";
                

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
        last_letter_count = 0;
        ResetResult();
    }

    public void ResetResult()
    {
        correct_img.SetActive(false);
        wrong_img.SetActive(false);
        deleteAll_Button.SetActive(false);
    }

    public void CheckTitle()
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
                Debug.Log("IS CORRECT");
                correct_img.SetActive(true);
            }
            else
            {
                Debug.Log("IS WRONG");
                wrong_img.SetActive(true);
            }

            deleteAll_Button.SetActive(true);
        }
    }

    public void BackButton()
    {
        SetPixelice(15);
        SceneManager.LoadScene(1);
    }


}
